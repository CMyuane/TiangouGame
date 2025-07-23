/**
 * 文件级说明文档
 *
 * 功能说明：
 * 该文件定义了AudioChannel类，用于管理同一通道内的多个音频轨道，实现轨道间的平滑切换。
 * 主要负责音频轨道的播放、停止、音量过渡等管理功能。
 * 输入：音频片段、播放参数（循环、音量等）、通道索引
 * 输出：管理后的音频轨道实例，实现轨道间的无缝切换
 * 外部交互：依赖AudioManager提供的协程管理，与AudioTrack类紧密交互，使用Unity的协程实现音量过渡
 *
 * 实现方式：
 * 采用管理器模式，负责同一通道内多个音频轨道的生命周期和状态管理
 * 类结构包含：
 * - 私有常量：轨道容器名称格式化字符串
 * - 公共属性：通道索引、轨道容器、当前激活轨道
 * - 私有字段：轨道列表、音量过渡协程
 * - 核心方法：播放轨道、获取轨道、设置激活轨道、音量过渡协程、销毁轨道、停止轨道
 * 关键逻辑位于VolumeLeveling协程（实现音量平滑过渡）和PlayTrack方法（轨道播放与切换）
 *
 * 设计思路与原因：
 * 1. 引入通道概念，允许同一通道内多个音频轨道共存并平滑切换（如背景音乐切换）
 * 2. 使用协程实现音量平滑过渡，避免音频切换时的突兀感
 * 3. 自动销毁音量为0的非激活轨道，优化资源占用
 * 约束：同一时间只有一个激活轨道，其他轨道会逐渐淡出
 * 替代方案考虑：曾考虑使用状态机管理轨道状态，但协程更适合处理渐变动画类逻辑
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音频通道类，管理同一通道内的多个音频轨道，实现轨道间的平滑切换
/// </summary>
public class AudioChannel
{
    /// <summary>
    /// 轨道容器名称格式化字符串，格式为"Channel - [通道索引]"
    /// </summary>
    private const string TRACK_CONTAINER_NAME_FORMAT = "Channel - [{0}]";

    /// <summary>
    /// 通道索引（只读），用于标识不同的音频通道
    /// </summary>
    public int channelIndex { get; private set; }

    /// <summary>
    /// 轨道容器（只读），用于存放该通道下的所有音频轨道游戏对象
    /// </summary>
    public Transform trackContainer { get; private set; } = null;

    /// <summary>
    /// 当前激活的音频轨道（只读），同一时间只有一个激活轨道
    /// </summary>
    public AudioTrack activeTrack { get; private set; } = null;

    /// <summary>
    /// 该通道下的所有音频轨道列表
    /// </summary>
    private List<AudioTrack> tracks = new List<AudioTrack>();

    /// <summary>
    /// 是否正在进行音量过渡（只读），通过检查协程是否存在判断
    /// </summary>
    private bool isLevelingVolume => co_volumeLeveling != null;

    /// <summary>
    /// 音量过渡协程的引用，用于控制协程的启动和停止
    /// </summary>
    private Coroutine co_volumeLeveling = null;

    /// <summary>
    /// 初始化音频通道实例
    /// </summary>
    /// <param name="channel">通道索引</param>
    public AudioChannel(int channel)
    {
        channelIndex = channel;

        // 创建轨道容器游戏对象，用于管理该通道下的所有音频轨道
        trackContainer = new GameObject(string.Format(TRACK_CONTAINER_NAME_FORMAT, channel)).transform;
        // 将轨道容器设置为AudioManager的子对象，便于统一管理
        trackContainer.SetParent(AudioManager.instance.transform);
    }

    /// <summary>
    /// 播放指定的音频轨道，若轨道已存在则直接播放，否则创建新轨道
    /// </summary>
    /// <param name="clip">音频片段</param>
    /// <param name="loop">是否循环播放</param>
    /// <param name="startingVolume">初始音量</param>
    /// <param name="volumeCap">音量上限</param>
    /// <param name="pitch">音高</param>
    /// <param name="filePath">音频文件路径</param>
    /// <returns>播放中的音频轨道实例</returns>
    public AudioTrack PlayTrack(AudioClip clip, bool loop, float startingVolume, float volumeCap, float pitch, string filePath)
    {
        // 检查轨道是否已存在
        if (TryGetTrack(clip.name, out AudioTrack existingTrack))
        {
            // 若轨道未在播放，则开始播放
            if (!existingTrack.isPlaying)
                existingTrack.Play();

            // 将该轨道设为激活轨道
            SetAsActiveTrack(existingTrack);

            return existingTrack;
        }

        // 创建新的音频轨道并播放
        AudioTrack track = new AudioTrack(clip, loop, startingVolume, volumeCap, pitch, this, AudioManager.instance.musicMixer, filePath);
        track.Play();

        // 将新轨道设为激活轨道
        SetAsActiveTrack(track);

        return track;
    }

    /// <summary>
    /// 尝试获取指定名称的音频轨道
    /// </summary>
    /// <param name="trackName">轨道名称</param>
    /// <param name="value">输出参数，找到的轨道实例（若存在）</param>
    /// <returns>是否找到轨道</returns>
    public bool TryGetTrack(string trackName, out AudioTrack value)
    {
        // 统一转为小写，实现不区分大小写的查找
        trackName = trackName.ToLower();

        // 遍历轨道列表查找匹配的轨道
        foreach (var track in tracks)
        {
            if (track.name.ToLower() == trackName)
            {
                value = track;
                return true;
            }
        }

        // 未找到轨道时返回null
        value = null;
        return false;
    }

    /// <summary>
    /// 将指定轨道设为激活轨道
    /// </summary>
    /// <param name="track">要设为激活的轨道</param>
    private void SetAsActiveTrack(AudioTrack track)
    {
        // 若轨道不在列表中，则添加到列表
        if (!tracks.Contains(track))
            tracks.Add(track);

        // 更新激活轨道
        activeTrack = track;

        // 尝试开始音量过渡
        TryStartVolumeLeveling();
    }

    /// <summary>
    /// 尝试启动音量过渡协程（若未正在过渡）
    /// </summary>
    private void TryStartVolumeLeveling()
    {
        if (!isLevelingVolume)
            co_volumeLeveling = AudioManager.instance.StartCoroutine(VolumeLeveling());
    }

    /// <summary>
    /// 音量过渡协程，实现激活轨道的淡入和其他轨道的淡出
    /// </summary>
    private IEnumerator VolumeLeveling()
    {
        // 循环条件：存在激活轨道且需要调整音量，或无激活轨道但有轨道需要淡出
        while ((activeTrack != null && (tracks.Count > 1 || activeTrack.volume != activeTrack.volumeCap)) ||
               (activeTrack == null && tracks.Count > 0))
        {
            // 反向遍历轨道列表，避免修改列表时出现索引问题
            for (int i = tracks.Count - 1; i >= 0; i--)
            {
                AudioTrack track = tracks[i];

                // 确定当前轨道的目标音量：激活轨道为音量上限，其他轨道为0
                float targetVol = activeTrack == track ? track.volumeCap : 0;

                // 若已达到目标音量，则跳过
                if (track == activeTrack && track.volume == targetVol)
                    continue;

                // 平滑过渡到目标音量
                track.volume = Mathf.MoveTowards(track.volume, targetVol, AudioManager.TRACK_TRANSITION_SPEED * Time.deltaTime);

                // 若非激活轨道且音量已为0，则销毁该轨道
                if (track != activeTrack && track.volume == 0)
                {
                    DestroyTrack(track);
                }
            }
            // 等待下一帧
            yield return null;
        }

        // 过渡完成，清除协程引用
        co_volumeLeveling = null;
    }

    /// <summary>
    /// 销毁指定的音频轨道
    /// </summary>
    /// <param name="track">要销毁的轨道</param>
    private void DestroyTrack(AudioTrack track)
    {
        // 从轨道列表中移除
        if (tracks.Contains(track))
            tracks.Remove(track);

        // 销毁轨道对应的游戏对象
        Object.Destroy(track.root);
    }

    /// <summary>
    /// 停止当前通道的激活轨道
    /// </summary>
    /// <param name="immediate">是否立即停止（true：立即销毁，false：平滑淡出）</param>
    public void StopTrack(bool immediate = false)
    {
        if (activeTrack == null)
            return;

        if (immediate)
        {
            // 立即销毁轨道
            DestroyTrack(activeTrack);
            activeTrack = null;
        }
        else
        {
            // 清除激活轨道，触发淡出效果
            activeTrack = null;
            TryStartVolumeLeveling();
        }
    }
}