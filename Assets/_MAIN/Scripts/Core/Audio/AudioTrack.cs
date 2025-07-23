/**
 * 文件级说明文档
 *
 * 功能说明：
 * 该文件定义了AudioTrack类，用于管理单个音频轨道的播放状态和属性。
 * 主要负责音频片段(AudioClip)的播放控制、音量调节、音高设置等核心功能。
 * 输入：音频片段(AudioClip)、循环状态、初始音量、音量上限、音高、所属音频通道(AudioChannel)、音频混合器组(AudioMixerGroup)、文件路径
 * 输出：可控制的音频播放实例，提供播放/停止等操作接口
 * 外部交互：依赖AudioChannel提供的轨道容器，使用Unity的AudioSource组件实现音频播放
 *
 * 实现方式：
 * 采用封装设计模式，将音频源(AudioSource)的操作封装为高层接口
 * 类结构包含：
 * - 私有常量：轨道名称格式化字符串
 * - 公共属性：轨道名称、路径、根游戏对象、循环状态、音量上限、音高、播放状态、音量
 * - 私有字段：所属音频通道、音频源组件
 * - 构造方法：初始化轨道属性并创建音频源
 * - 核心方法：创建音频源、播放、停止
 * 关键逻辑位于构造方法(初始化)和VolumeLeveling协程(音量平滑过渡)中
 *
 * 设计思路与原因：
 * 1. 封装AudioSource组件，提供更简洁的音频控制接口，降低使用复杂度
 * 2. 通过关联AudioChannel实现多轨道管理，支持轨道间的音量平滑过渡
 * 3. 将音频源作为子对象挂到通道容器下，便于层级管理和资源释放
 * 约束：依赖Unity的AudioSource组件，仅能在Unity引擎中使用
 * 替代方案考虑：曾考虑使用静态方法管理音频，但为了支持多实例和状态保持，最终采用类实例化方式
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 音频轨道类，管理单个音频片段的播放状态和属性
/// </summary>
public class AudioTrack
{
    /// <summary>
    /// 轨道名称格式化字符串，格式为"Track - [轨道名]"
    /// </summary>
    private const string TRACK_NAME_FORMAT = "Track - [{0}]";

    /// <summary>
    /// 轨道名称（只读）
    /// </summary>
    public string name { get; private set; }

    /// <summary>
    /// 音频文件路径（只读）
    /// </summary>
    public string path { get; private set; }

    /// <summary>
    /// 轨道根游戏对象（只读），返回音频源所在的游戏对象
    /// </summary>
    public GameObject root => source.gameObject;

    /// <summary>
    /// 所属的音频通道
    /// </summary>
    private AudioChannel channel;

    /// <summary>
    /// 音频源组件，负责实际的音频播放
    /// </summary>
    private AudioSource source;

    /// <summary>
    /// 轨道是否循环播放（只读），映射音频源的循环状态
    /// </summary>
    public bool loop => source.loop;

    /// <summary>
    /// 音量上限（只读），用于限制轨道的最大音量
    /// </summary>
    public float volumeCap { get; private set; }

    /// <summary>
    /// 音高属性，可读写，映射到音频源的音高
    /// </summary>
    public float pitch
    { get { return source.pitch; } set { source.pitch = value; } }

    /// <summary>
    /// 轨道是否正在播放（只读），映射音频源的播放状态
    /// </summary>
    public bool isPlaying => source.isPlaying;

    /// <summary>
    /// 音量属性，可读写，映射到音频源的音量
    /// </summary>
    public float volume
    { get { return source.volume; } set { source.volume = value; } }

    /// <summary>
    /// 初始化音频轨道实例
    /// </summary>
    /// <param name="clip">音频片段</param>
    /// <param name="loop">是否循环播放</param>
    /// <param name="startingVolume">初始音量</param>
    /// <param name="volumeCap">音量上限</param>
    /// <param name="pitch">音高</param>
    /// <param name="channel">所属音频通道</param>
    /// <param name="mixer">音频混合器组</param>
    /// <param name="filePath">音频文件路径</param>
    public AudioTrack(AudioClip clip, bool loop, float startingVolume, float volumeCap, float pitch, AudioChannel channel, AudioMixerGroup mixer, string filePath)
    {
        // 初始化轨道名称和路径
        name = clip.name;
        path = filePath;

        // 保存所属通道和音量上限
        this.channel = channel;
        this.volumeCap = volumeCap;

        // 创建音频源并设置相关属性
        source = CreateSource();
        source.clip = clip;
        source.loop = loop;
        source.volume = startingVolume;
        source.pitch = pitch;

        // 设置音频输出到指定的混合器组
        source.outputAudioMixerGroup = mixer;
    }

    /// <summary>
    /// 创建并配置音频源组件
    /// </summary>
    /// <returns>创建的音频源组件</returns>
    private AudioSource CreateSource()
    {
        // 创建新的游戏对象作为音频源的载体
        GameObject go = new GameObject(string.Format(TRACK_NAME_FORMAT, name));
        // 将游戏对象设置为通道容器的子对象，便于管理
        go.transform.SetParent(channel.trackContainer);
        // 为游戏对象添加音频源组件
        AudioSource source = go.AddComponent<AudioSource>();

        return source;
    }

    /// <summary>
    /// 播放音频轨道
    /// </summary>
    public void Play()
    {
        source.Play();
    }

    /// <summary>
    /// 停止音频轨道播放
    /// </summary>
    public void Stop()
    {
        source.Stop();
    }
}