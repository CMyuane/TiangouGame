/**
 * 文件级说明文档
 *
 * 功能说明：
 * 该文件定义了AudioManager类，作为音频系统的核心管理器，负责全局音频资源的加载、播放、停止和音量控制。
 * 支持背景音乐轨道管理、音效播放、语音播放等功能，并提供统一的音量控制接口。
 * 输入：音频文件路径、播放参数（音量、音高、循环等）、通道索引
 * 输出：播放中的音频源或轨道实例，全局音频状态的改变
 * 外部交互：与AudioChannel和AudioTrack类协作管理音频播放，使用Unity的Resources加载音频资源，通过AudioMixer控制音量
 *
 * 实现方式：
 * 采用单例模式确保全局唯一的音频管理器实例，统一管理所有音频资源和播放状态
 * 类结构包含：
 * - 公共常量：音量参数名称、静音音量、音效名称格式、轨道过渡速度
 * - 静态属性：单例实例
 * - 公共字段：音频通道字典、各种音频混合器组、音量衰减曲线
 * - 私有字段：音效根节点
 * - 核心方法：播放音效/语音/轨道、停止音效/轨道、获取音频通道、设置各类音量
 * 关键逻辑位于PlaySoundEffect（音效播放）和PlayTrack（轨道播放）方法
 *
 * 设计思路与原因：
 * 1. 单例模式确保全局唯一的音频管理入口，避免多实例冲突
 * 2. 分离背景音乐（轨道）、音效和语音，使用不同的混合器组实现独立音量控制
 * 3. 采用资源路径加载方式，简化音频资源的引用和管理
 * 4. 自动销毁非循环音效，优化内存使用
 * 约束：音频资源必须放在Resources目录下才能被正确加载
 * 替代方案考虑：曾考虑使用Addressables管理音频资源，但为简化实现采用了Resources加载
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 音频管理器，作为全局音频系统的核心，负责管理所有音频的播放、停止和音量控制
/// 采用单例模式确保全局唯一实例
/// </summary>
public class AudioManager : MonoBehaviour
{
    /// <summary>
    /// 音乐音量参数名称，对应音频混合器中的参数
    /// </summary>
    public const string MUSIC_VOLUME_PARAMETER_NAME = "MusicVolume";

    /// <summary>
    /// 音效音量参数名称，对应音频混合器中的参数
    /// </summary>
    public const string SFX_VOLUME_PARAMETER_NAME = "SFXVolume";

    /// <summary>
    /// 语音音量参数名称，对应音频混合器中的参数
    /// </summary>
    public const string VOICES_VOLUME_PARAMETER_NAME = "VoicesVolume";

    /// <summary>
    /// 静音时的音量水平（分贝）
    /// </summary>
    public const float MUTED_VOLUME_LEVEL = -80f;

    /// <summary>
    /// 音效父对象名称
    /// </summary>
    private const string SFX_PARENT_NAME = "SFX";

    /// <summary>
    /// 音效名称格式中的容器字符，用于标识音效名称
    /// </summary>
    public static char[] SFX_NAME_FORMAT_CONTAINERS = new char[] { '[', ']' };

    /// <summary>
    /// 音效名称格式化字符串，格式为"SFX - [音效名]"
    /// </summary>
    private static string SFX_NAME_FORMAT = $"SFX - {SFX_NAME_FORMAT_CONTAINERS[0]}" + "{0}" + $"{SFX_NAME_FORMAT_CONTAINERS[1]}";

    /// <summary>
    /// 轨道音量过渡速度，控制轨道切换时的淡入淡出速度
    /// </summary>
    public const float TRACK_TRANSITION_SPEED = 1f;

    /// <summary>
    /// 音频管理器单例实例（只读）
    /// </summary>
    public static AudioManager instance { get; private set; }

    /// <summary>
    /// 音频通道字典，键为通道索引，值为对应的音频通道实例
    /// </summary>
    public Dictionary<int, AudioChannel> channels = new Dictionary<int, AudioChannel>();

    /// <summary>
    /// 音乐音频混合器组，用于控制所有音乐轨道的音量
    /// </summary>
    public AudioMixerGroup musicMixer;

    /// <summary>
    /// 音效音频混合器组，用于控制所有音效的音量
    /// </summary>
    public AudioMixerGroup sfxMixer;

    /// <summary>
    /// 语音音频混合器组，用于控制所有语音的音量
    /// </summary>
    public AudioMixerGroup voicesMixer;

    /// <summary>
    /// 音频衰减曲线，用于将0-1范围的音量值转换为混合器使用的分贝值
    /// </summary>
    public AnimationCurve audioFalloffCurve;

    /// <summary>
    /// 音效根节点，所有音效游戏对象的父对象，用于层级管理
    /// </summary>
    private Transform sfxRoot;

    /// <summary>
    /// 所有音效音频源（只读），返回所有子对象中的AudioSource组件
    /// </summary>
    public AudioSource[] allSFX => sfxRoot.GetComponentsInChildren<AudioSource>();

    /// <summary>
    /// 初始化音频管理器，确保单例实例
    /// </summary>
    private void Awake()
    {
        // 单例模式实现：若实例不存在则创建，否则销毁重复实例
        if (instance == null)
        {
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }

        // 创建音效根节点，用于管理所有音效游戏对象
        sfxRoot = new GameObject(SFX_PARENT_NAME).transform;
        sfxRoot.SetParent(transform);
    }

    /// <summary>
    /// 通过文件路径播放音效
    /// </summary>
    /// <param name="filePath">音效文件在Resources目录下的路径</param>
    /// <param name="mixer">音频混合器组（默认为音效混合器）</param>
    /// <param name="volume">音量（0-1范围）</param>
    /// <param name="pitch">音高（1为正常）</param>
    /// <param name="loop">是否循环播放</param>
    /// <returns>创建的音频源组件</returns>
    public AudioSource PlaySoundEffect(string filePath, AudioMixerGroup mixer = null, float volume = 1, float pitch = 1, bool loop = false)
    {
        // 从Resources目录加载音频片段
        AudioClip clip = Resources.Load<AudioClip>(filePath);

        // 加载失败时输出错误信息
        if (clip == null)
        {
            Debug.LogError($"无法从'{filePath}'加载文件. 确保路径正确和文件存在");
            return null;
        }

        // 调用重载方法播放音效
        return PlaySoundEffect(clip, mixer, volume, pitch, loop, filePath);
    }

    /// <summary>
    /// 通过音频片段直接播放音效
    /// </summary>
    /// <param name="clip">音频片段</param>
    /// <param name="mixer">音频混合器组（默认为音效混合器）</param>
    /// <param name="volume">音量（0-1范围）</param>
    /// <param name="pitch">音高（1为正常）</param>
    /// <param name="loop">是否循环播放</param>
    /// <param name="filePath">音频文件路径（可选）</param>
    /// <returns>创建的音频源组件</returns>
    public AudioSource PlaySoundEffect(AudioClip clip, AudioMixerGroup mixer = null, float volume = 1, float pitch = 1, bool loop = false, string filePath = "")
    {
        // 确定音效名称（优先使用文件路径，其次使用音频片段名称）
        string fileName = clip.name;
        if (filePath != string.Empty)
            fileName = filePath;

        // 创建音效游戏对象并添加音频源组件
        AudioSource effectSource = new GameObject(string.Format(SFX_NAME_FORMAT, fileName)).AddComponent<AudioSource>();
        // 设置为音效根节点的子对象
        effectSource.transform.SetParent(sfxRoot);
        // 与根节点位置保持一致（2D音效）
        effectSource.transform.position = sfxRoot.position;

        // 设置音频源属性
        effectSource.clip = clip;

        // 若未指定混合器，则使用默认音效混合器
        if (mixer == null)
            mixer = sfxMixer;

        effectSource.outputAudioMixerGroup = mixer;
        effectSource.volume = volume;
        effectSource.spatialBlend = 0; // 0表示2D音效，不随位置变化
        effectSource.pitch = pitch;
        effectSource.loop = loop;

        // 开始播放
        effectSource.Play();

        // 非循环音效在播放完成后自动销毁（额外加1秒缓冲）
        if (!loop)
            Destroy(effectSource.gameObject, (clip.length / pitch) + 1);

        return effectSource;
    }

    /// <summary>
    /// 通过文件路径播放语音
    /// </summary>
    /// <param name="filePath">语音文件在Resources目录下的路径</param>
    /// <param name="volume">音量（0-1范围）</param>
    /// <param name="pitch">音高（1为正常）</param>
    /// <param name="loop">是否循环播放</param>
    /// <returns>创建的音频源组件</returns>
    public AudioSource PlayVoice(string filePath, float volume = 1, float pitch = 1, bool loop = false)
    {
        // 调用音效播放方法，指定语音混合器
        return PlaySoundEffect(filePath, voicesMixer, volume, pitch, loop);
    }

    /// <summary>
    /// 通过音频片段直接播放语音
    /// </summary>
    /// <param name="clip">音频片段</param>
    /// <param name="volume">音量（0-1范围）</param>
    /// <param name="pitch">音高（1为正常）</param>
    /// <param name="loop">是否循环播放</param>
    /// <returns>创建的音频源组件</returns>
    public AudioSource PlayVoice(AudioClip clip, float volume = 1, float pitch = 1, bool loop = false)
    {
        // 调用音效播放方法，指定语音混合器
        return PlaySoundEffect(clip, voicesMixer, volume, pitch, loop);
    }

    /// <summary>
    /// 停止指定音频片段的音效播放（重载方法）
    /// </summary>
    /// <param name="clip">要停止的音频片段</param>
    public void StopSoundEffect(AudioClip clip) => StopSoundEffect(clip.name);

    /// <summary>
    /// 停止指定名称的音效播放
    /// </summary>
    /// <param name="soundName">音效名称</param>
    public void StopSoundEffect(string soundName)
    {
        soundName = soundName.ToLower();

        // 查找所有音效音频源
        AudioSource[] sources = sfxRoot.GetComponentsInChildren<AudioSource>();
        foreach (var source in sources)
        {
            // 找到匹配的音效并销毁
            if (source.clip.name.ToLower() == soundName)
            {
                Destroy(source.gameObject);
                return;
            }
        }
    }

    /// <summary>
    /// 检查指定名称的音效是否正在播放
    /// </summary>
    /// <param name="soundName">音效名称</param>
    /// <returns>是否正在播放</returns>
    public bool IsPlayingSoundEffect(string soundName)
    {
        soundName = soundName.ToLower();

        // 查找所有音效音频源
        AudioSource[] sources = sfxRoot.GetComponentsInChildren<AudioSource>();
        foreach (var source in sources)
        {
            // 检查是否有匹配的正在播放的音效
            if (source.clip.name.ToLower() == soundName)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 通过文件路径播放音频轨道（通常用于背景音乐）
    /// </summary>
    /// <param name="filePath">音频文件在Resources目录下的路径</param>
    /// <param name="channel">通道索引（默认为0）</param>
    /// <param name="loop">是否循环播放（默认为true）</param>
    /// <param name="startingVolume">初始音量（默认为0，用于淡入效果）</param>
    /// <param name="volumeCap">音量上限（默认为1）</param>
    /// <param name="pitch">音高（默认为1）</param>
    /// <returns>创建的音频轨道实例</returns>
    public AudioTrack PlayTrack(string filePath, int channel = 0, bool loop = true, float startingVolume = 0f, float volumeCap = 1f, float pitch = 1f)
    {
        // 从Resources目录加载音频片段
        AudioClip clip = Resources.Load<AudioClip>(filePath);

        // 加载失败时输出错误信息
        if (clip == null)
        {
            Debug.LogError($"无法从 '{filePath}'加载文件. 确保路径正确和文件存在");
            return null;
        }

        // 调用重载方法播放轨道
        return PlayTrack(clip, channel, loop, startingVolume, volumeCap, pitch, filePath);
    }

    /// <summary>
    /// 通过音频片段直接播放音频轨道（通常用于背景音乐）
    /// </summary>
    /// <param name="clip">音频片段</param>
    /// <param name="channel">通道索引（默认为0）</param>
    /// <param name="loop">是否循环播放（默认为true）</param>
    /// <param name="startingVolume">初始音量（默认为0，用于淡入效果）</param>
    /// <param name="volumeCap">音量上限（默认为1）</param>
    /// <param name="pitch">音高（默认为1）</param>
    /// <param name="filePath">音频文件路径（可选）</param>
    /// <returns>创建的音频轨道实例</returns>
    public AudioTrack PlayTrack(AudioClip clip, int channel = 0, bool loop = true, float startingVolume = 0f, float volumeCap = 1f, float pitch = 1f, string filePath = "")
    {
        // 获取或创建指定通道
        AudioChannel audioChannel = TryGetChannel(channel, createIfDoesNotExist: true);
        // 在通道中播放轨道
        AudioTrack track = audioChannel.PlayTrack(clip, loop, startingVolume, volumeCap, pitch, filePath);
        return track;
    }

    /// <summary>
    /// 停止指定通道的当前轨道
    /// </summary>
    /// <param name="channel">通道索引</param>
    public void StopTrack(int channel)
    {
        // 获取指定通道（不自动创建）
        AudioChannel c = TryGetChannel(channel, createIfDoesNotExist: false);

        if (c == null)
            return;

        // 停止通道的当前轨道
        c.StopTrack();
    }

    /// <summary>
    /// 停止指定名称的轨道
    /// </summary>
    /// <param name="trackName">轨道名称</param>
    public void StopTrack(string trackName)
    {
        trackName = trackName.ToLower();

        // 遍历所有通道查找并停止指定轨道
        foreach (var channel in channels.Values)
        {
            if (channel.activeTrack != null && channel.activeTrack.name.ToLower() == trackName)
            {
                channel.StopTrack();
                return;
            }
        }
    }

    /// <summary>
    /// 停止所有通道的轨道
    /// </summary>
    public void StopAllTracks()
    {
        foreach (AudioChannel channel in channels.Values)
        {
            channel.StopTrack();
        }
    }

    /// <summary>
    /// 停止所有正在播放的音效
    /// </summary>
    public void StopAllSoundEffects()
    {
        // 销毁所有音效游戏对象
        AudioSource[] sources = sfxRoot.GetComponentsInChildren<AudioSource>();
        foreach (var source in sources)
        {
            Destroy(source.gameObject);
        }
    }

    /// <summary>
    /// 获取或创建指定的音频通道
    /// </summary>
    /// <param name="channelNumber">通道索引</param>
    /// <param name="createIfDoesNotExist">若不存在是否创建（默认为false）</param>
    /// <returns>音频通道实例（若不存在且不创建则返回null）</returns>
    public AudioChannel TryGetChannel(int channelNumber, bool createIfDoesNotExist = false)
    {
        AudioChannel channel = null;

        // 尝试从字典中获取通道
        if (channels.TryGetValue(channelNumber, out channel))
        {
            return channel;
        }
        // 若需要创建且不存在，则创建新通道
        else if (createIfDoesNotExist)
        {
            channel = new AudioChannel(channelNumber);
            channels.Add(channelNumber, channel);
            return channel;
        }

        return null;
    }

    /// <summary>
    /// 设置音乐音量
    /// </summary>
    /// <param name="volume">音量值（0-1范围）</param>
    /// <param name="muted">是否静音</param>
    public void SetMusicVolume(float volume, bool muted)
    {
        // 根据静音状态和衰减曲线计算最终音量（分贝值）
        volume = muted ? MUTED_VOLUME_LEVEL : audioFalloffCurve.Evaluate(volume);
        musicMixer.audioMixer.SetFloat(MUSIC_VOLUME_PARAMETER_NAME, volume);
    }

    /// <summary>
    /// 设置音效音量
    /// </summary>
    /// <param name="volume">音量值（0-1范围）</param>
    /// <param name="muted">是否静音</param>
    public void SetSFXVolume(float volume, bool muted)
    {
        // 根据静音状态和衰减曲线计算最终音量（分贝值）
        volume = muted ? MUTED_VOLUME_LEVEL : audioFalloffCurve.Evaluate(volume);
        sfxMixer.audioMixer.SetFloat(SFX_VOLUME_PARAMETER_NAME, volume);
    }

    /// <summary>
    /// 设置语音音量
    /// </summary>
    /// <param name="volume">音量值（0-1范围）</param>
    /// <param name="muted">是否静音</param>
    public void SetVoicesVolume(float volume, bool muted)
    {
        // 根据静音状态和衰减曲线计算最终音量（分贝值）
        volume = muted ? MUTED_VOLUME_LEVEL : audioFalloffCurve.Evaluate(volume);
        voicesMixer.audioMixer.SetFloat(VOICES_VOLUME_PARAMETER_NAME, volume);
    }
}