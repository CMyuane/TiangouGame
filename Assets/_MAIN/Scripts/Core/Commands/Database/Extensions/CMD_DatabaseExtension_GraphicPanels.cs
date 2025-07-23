using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace COMMANDS
{
    public class CMD_DatabaseExtension_GraphicPanels : CMD_DatabaseExtension
    {
        private static string[] PARAM_PANEL = new string[] { "-p", "-panel", "面板" };
        private static string[] PARAM_LAYER = new string[] { "-l", "-layer", "层" };
        private static string[] PARAM_MEDIA = new string[] { "-m", "-media", "媒体" };
        private static string[] PARAM_SPEED = new string[] { "-spd", "-speed", "速度" };
        private static string[] PARAM_IMMEDIATE = new string[] { "-i", "-immediate", "立即" };
        private static string[] PARAM_BLENDTEX = new string[] { "-b", "-blend", "纹理" };
        private static string[] PARAM_USEVIDEOAUDIO = new string[] { "-aud", "-audio", "音频" };

        // 以~开头会识别资源文件夹根路径
        private const string HOME_DIRECTORY_SYMBOL = "~/";

        public new static void Extend(CommandDatabase database)
        {
            database.AddCommand("setlayermedia", new Func<string[], IEnumerator>(SetLayerMedia));
            database.AddCommand("clearlayermedia", new Func<string[], IEnumerator>(ClearLayerMedia));
        }

        private static IEnumerator SetLayerMedia(string[] data)
        {
            //可用的参数
            string panelName = "";
            int layer = 0;
            string mediaName = "";
            float transitionSpeed = 0;
            bool immediate = false;
            string blendTexName = "";
            bool useAudio = false;

            string pathToGraphic = "";
            UnityEngine.Object graphic = null;
            Texture blendTex = null;

            //获取参数
            var parameters = ConvertDataToParameters(data);

            //尝试获取该媒体所应用的面板
            parameters.TryGetValue(PARAM_PANEL, out panelName);
            GraphicPanel panel = GraphicPanelManager.instance.GetPanel(panelName);
            if (panel == null)
            {
                Debug.LogError($"无法获取 '{panelName}'，因为他无效，检查面板名或者命令");
                yield break;
            }

            //尝试获取该媒体所应用的层
            parameters.TryGetValue(PARAM_LAYER, out layer, defaultValue: 0);

            //尝试获取该媒体所应用的媒体名称
            parameters.TryGetValue(PARAM_MEDIA, out mediaName);

            //尝试获取该媒体是否立即显示
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            // 尝试获取该媒体所应用的过渡速度
            if (!immediate)
                parameters.TryGetValue(PARAM_SPEED, out transitionSpeed, defaultValue: 1);

            // 尝试获取该媒体所应用的纹理名称
            parameters.TryGetValue(PARAM_BLENDTEX, out blendTexName);

            // 尝试获取该媒体所应用的音频
            parameters.TryGetValue(PARAM_USEVIDEOAUDIO, out useAudio, defaultValue: false);

            // 从路径中读取资源文件
            pathToGraphic = FilePaths.GetPathToResource(FilePaths.resources_backgroundImages, mediaName);
            graphic = Resources.Load<Texture>(pathToGraphic);

            if (graphic == null)
            {
                pathToGraphic = FilePaths.GetPathToResource(FilePaths.resources_backgroundVideos, mediaName);
                graphic = Resources.Load<VideoClip>(pathToGraphic);
            }

            if (graphic == null)
            {
                Debug.LogError($"在资源文件夹无法找到 '{mediaName}'，确保路径正确以及文件存在 ");
                yield break;
            }

            if (!immediate && blendTexName != string.Empty)
                blendTex = Resources.Load<Texture>(FilePaths.resources_blendTextures + blendTexName);

            //获取应用媒体的图层
            GraphicLayer graphicLayer = panel.GetLayer(layer, createIfDoesNotExist: true);

            if (graphic is Texture)
            {
                yield return graphicLayer.SetTexture(graphic as Texture, transitionSpeed, blendTex, pathToGraphic, immediate);
            }
            else
            {
                yield return graphicLayer.SetVideo(graphic as VideoClip, transitionSpeed, useAudio, blendTex, pathToGraphic, immediate);
            }
        }

        private static IEnumerator ClearLayerMedia(string[] data)
        {
            //可用的参数
            string panelName = "";
            int layer = 0;
            float transitionSpeed = 0;
            bool immediate = false;
            string blendTexName = "";

            Texture blendTex = null;

            //获取参数
            var parameters = ConvertDataToParameters(data);

            //尝试获取该媒体所应用的面板
            parameters.TryGetValue(PARAM_PANEL, out panelName);
            GraphicPanel panel = GraphicPanelManager.instance.GetPanel(panelName);
            if (panel == null)
            {
                Debug.LogError($"无法获取 '{panelName}'，因为他无效，检查面板名或者命令");
                yield break;
            }

            //尝试获取该媒体所应用的层
            parameters.TryGetValue(PARAM_LAYER, out layer, defaultValue: -1);

            //尝试获取该媒体是否立即显示
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            // 尝试获取该媒体所应用的过渡速度
            if (!immediate)
                parameters.TryGetValue(PARAM_SPEED, out transitionSpeed, defaultValue: 1);

            // 尝试获取该媒体所应用的纹理名称
            parameters.TryGetValue(PARAM_BLENDTEX, out blendTexName);

            if (!immediate && blendTexName != string.Empty)
                blendTex = Resources.Load<Texture>(FilePaths.resources_blendTextures + blendTexName);

            if (layer == -1)
                panel.Clear(transitionSpeed, blendTex, immediate);
            else
            {
                GraphicLayer graphicLayer = panel.GetLayer(layer);
                if (graphicLayer == null)
                {
                    Debug.LogError($"无法清除面板'{panel.panelName}'上的图层 [{layer}]，因为他无效，检查图层号或者命令");
                    yield break;
                }
                graphicLayer.Clear(transitionSpeed, blendTex, immediate);
            }
        }

        private static string GetPathToGraphic(string defaultPath, string graphicName)
        {
            //如果路径以~/开头，则将其替换为默认路径
            if (graphicName.StartsWith(HOME_DIRECTORY_SYMBOL))

                return graphicName.Substring(HOME_DIRECTORY_SYMBOL.Length);

            return defaultPath + graphicName;
        }
    }
}