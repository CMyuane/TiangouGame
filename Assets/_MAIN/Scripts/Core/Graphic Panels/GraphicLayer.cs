﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class GraphicLayer
{
    public const string LAYER_OBJECT_NAME_FORMAT = "Layer: {0}";
    public int layerDepth = 0;
    public Transform panel;

    //public GraphicObject currentGraphic { get; private set; } = null;
    public GraphicObject currentGraphic = null;

    private List<GraphicObject> oldGraphics = new List<GraphicObject>();

    public Coroutine SetTexture(string filePath, float transitionSpeed = 1f, Texture blendingTexture = null, bool immediate = false)
    {
        Texture tex = Resources.Load<Texture>(filePath);

        if (tex == null)
        {
            Debug.LogError($"无法从{filePath}中加载图像，检查其是否包含在路径内");
            return null;
        }

        return SetTexture(tex, transitionSpeed, blendingTexture, filePath);
    }

    public Coroutine SetTexture(Texture tex, float transitionSpeed = 1f, Texture blendingTexture = null, string filePath = "", bool immediate = false)
    {
        return CreateGraphic(tex, transitionSpeed, filePath, blendingTexture: blendingTexture, immediate: immediate);
    }

    public Coroutine SetVideo(string filePath, float transitionSpeed = 1f, bool useAudio = true, Texture blendingTexture = null, bool immediate = false)
    {
        VideoClip clip = Resources.Load<VideoClip>(filePath);

        if (clip == null)
        {
            Debug.LogError($"无法从{filePath}中加载ship，检查其是否包含在路径内");
            return null;
        }

        return SetVideo(clip, transitionSpeed, useAudio, blendingTexture, filePath);
    }

    public Coroutine SetVideo(VideoClip video, float transitionSpeed = 1f, bool useAudio = true, Texture blendingTexture = null, string filePath = "", bool immediate = false)
    {
        return CreateGraphic(video, transitionSpeed, filePath, useAudio, blendingTexture, immediate);
    }

    private Coroutine CreateGraphic<T>(T graphicData, float transitionSpeed, string filePath, bool useAudioForVideo = true, Texture blendingTexture = null, bool immediate = false)
    {
        GraphicObject newGraphic = null;

        if (graphicData is Texture)
            newGraphic = new GraphicObject(this, filePath, graphicData as Texture, immediate);
        else if (graphicData is VideoClip)
            newGraphic = new GraphicObject(this, filePath, graphicData as VideoClip, useAudioForVideo, immediate);

        if (currentGraphic != null && !oldGraphics.Contains(currentGraphic)) //这是为了避免重复添加当前图形到旧图形列表中
            oldGraphics.Add(currentGraphic);

        currentGraphic = newGraphic;

        if (!immediate)
            return currentGraphic.FadeIn(transitionSpeed, blendingTexture);

        //其他图形淡出
        DestroyOldGraphics();
        return null;
    }

    public void DestroyOldGraphics()
    {
        foreach (var g in oldGraphics)
            Object.Destroy(g.renderer.gameObject);

        oldGraphics.Clear();
    }

    public void Clear(float transitionSpeed = 1, Texture blendTexture = null, bool immediate = false)
    {
        if (currentGraphic != null)
        {
            if (!immediate)
                currentGraphic.FadeOut(transitionSpeed, blendTexture);
            else
                currentGraphic.Destroy();
        }

        foreach (var g in oldGraphics)
        {
            if (!immediate)
                g.FadeOut(transitionSpeed, blendTexture);
            else
                g.Destroy();
        }
    }
}