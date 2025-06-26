using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// 角色精灵类：继承自Character基类，处理精灵角色的渲染和动画
namespace CHARACTERS
{
    public class Character_Sprite : Character
    {
        // 常量定义
        private const string SPRITE_RENDERER_PARENT_NAME = "Renderers"; // 精灵渲染器父对象名称
        private const string SPRITESHEET_DEFAULT_SHEETNAME = "Default"; // 默认精灵表名称
        private const char SPRITESHEET_TEX_SPRITE_DELIMITTER = '-'; // 精灵表纹理与精灵名称分隔符

        // 引用组件
        private CanvasGroup rootCG => root.GetComponent<CanvasGroup>(); // 根对象的CanvasGroup组件

        // 精灵层管理
        public List<CharacterSpriteLayer> layers = new List<CharacterSpriteLayer>(); // 精灵层列表

        // 资源路径
        private string artAssetsDirectory = ""; // 艺术资源目录路径

        // 角色可见性属性
        public override bool isVisible
        {
            get { return isRevealing || rootCG.alpha > 0; } // 判断是否可见
            set { rootCG.alpha = value ? 1 : 0; } // 设置透明度
        }

        // 构造函数
        public Character_Sprite(string name, CharacterConfigData config, GameObject prefab, string rootAssetsFolder)
            : base(name, config, prefab) // 调用基类构造函数
        {
            // 初始化角色透明度（根据是否启用）
            rootCG.alpha = ENABLE_ON_START ? 1 : 0;

            // 设置艺术资源目录路径
            artAssetsDirectory = rootAssetsFolder + "/Images";

            // 获取并初始化精灵层
            GetLayers();
        }

        // 获取所有精灵层
        private void GetLayers()
        {
            // 查找渲染器根对象
            Transform rendererRoot = animator.transform.Find(SPRITE_RENDERER_PARENT_NAME);
            if (rendererRoot == null) return;

            // 遍历所有子对象
            for (int i = 0; i < rendererRoot.transform.childCount; i++)
            {
                Transform child = rendererRoot.transform.GetChild(i);

                // 获取图像组件
                Image rendererImage = child.GetComponentInChildren<Image>();

                if (rendererImage != null)
                {
                    // 创建精灵层并添加到列表
                    CharacterSpriteLayer layer = new CharacterSpriteLayer(rendererImage, i);
                    layers.Add(layer);

                    // 重命名子对象以便调试
                    child.name = $"Layer: {i}";
                }
            }
        }

        // 设置指定层的精灵
        public void SetSprite(Sprite sprite, int layer = 0)
        {
            layers[layer].SetSprite(sprite);
        }

        // 获取精灵资源
        public Sprite GetSprite(string spriteName)
        {
            // 处理精灵表类型角色
            if (config.characterType == CharacterType.SpriteSheet)
            {
                // 分割纹理名和精灵名
                string[] data = spriteName.Split(SPRITESHEET_TEX_SPRITE_DELIMITTER);
                Sprite[] spriteArray = new Sprite[0];

                if (data.Length == 2)
                {
                    // 提取纹理名称和精灵名称
                    string textureName = data[0];
                    spriteName = data[1];

                    // 加载精灵表
                    spriteArray = Resources.LoadAll<Sprite>($"{artAssetsDirectory}/{textureName}");
                }
                else
                {
                    // 使用默认精灵表
                    spriteArray = Resources.LoadAll<Sprite>($"{artAssetsDirectory}/{SPRITESHEET_DEFAULT_SHEETNAME}");
                }

                // 检查是否加载成功
                if (spriteArray.Length == 0)
                    Debug.LogWarning($"角色 '{name}' 没有默认的精灵表，无法获取精灵 '{SPRITESHEET_DEFAULT_SHEETNAME}'");

                // 查找匹配名称的精灵
                return Array.Find(spriteArray, sprite => sprite.name == spriteName);
            }
            else // 普通精灵类型
            {
                // 直接加载单个精灵
                return Resources.Load<Sprite>($"{artAssetsDirectory}/{spriteName}");
            }
        }

        // 精灵过渡动画
        public Coroutine TransitionSprite(Sprite sprite, int layer = 0, float speed = 1)
        {
            CharacterSpriteLayer spriteLayer = layers[layer];
            Debug.Log($"TransitionSprite called with layer: {layer}");
            return spriteLayer.TransitionSprite(sprite, speed);
        }

        // 显示/隐藏角色的协程
        public override IEnumerator ShowingOrHiding(bool show, float speedMultiplier = 1f)
        {
            // 设置目标透明度
            float targetAlpha = show ? 1f : 0;
            CanvasGroup self = rootCG;

            // 渐变调整透明度
            while (self.alpha != targetAlpha)
            {
                self.alpha = Mathf.MoveTowards(self.alpha, targetAlpha,
                    3f * Time.deltaTime * speedMultiplier);
                yield return null;
            }

            // 重置协程引用
            co_revealing = null;
            co_hiding = null;
        }

        // 设置角色颜色
        public override void SetColor(Color color)
        {
            base.SetColor(color); // 调用基类方法

            // 应用到所有精灵层
            foreach (CharacterSpriteLayer layer in layers)
            {
                layer.StopChangingColor(); // 停止正在进行的颜色变化
                layer.SetColor(displayColor); // 设置新颜色
            }
        }

        // 颜色变化动画
        public override IEnumerator ChangingColor(float speed)
        {
            // 为所有层启动颜色过渡
            foreach (CharacterSpriteLayer layer in layers)
                layer.TransitionColor(displayColor, speed);

            yield return null;

            // 等待所有层完成颜色变化
            while (layers.Any(l => l.isChangingColor))
                yield return null;

            // 重置协程引用
            co_changingColor = null;
        }

        // 高亮角色动画
        public override IEnumerator Highlighting(float speedMultiplier, bool immediate = false)
        {
            // 应用到所有精灵层
            foreach (CharacterSpriteLayer layer in layers)
            {
                if (immediate)
                    layer.SetColor(displayColor); // 立即设置颜色
                else
                    layer.TransitionColor(displayColor, speedMultiplier); // 渐变设置颜色
            }

            yield return null;

            // 等待所有层完成颜色变化
            while (layers.Any(l => l.isChangingColor))
                yield return null;

            // 重置协程引用
            co_highlighting = null;
        }

        // 改变角色朝向
        public override IEnumerator FaceDirection(bool faceLeft, float speedMultiplier, bool immediate)
        {
            // 为所有层设置朝向
            foreach (CharacterSpriteLayer layer in layers)
            {
                if (faceLeft)
                    layer.FaceLeft(speedMultiplier, immediate); // 朝左
                else
                    layer.FaceRight(speedMultiplier, immediate); // 朝右
            }

            yield return null;

            // 等待所有层完成翻转
            while (layers.Any(l => l.isFlipping))
                yield return null;

            // 重置协程引用
            co_flipping = null;
        }

        // 处理角色表情指令
        public override void OnReceiveCastingExpression(int layer, string expression)
        {
            // 获取对应表情的精灵
            Sprite sprite = GetSprite(expression);

            if (sprite == null)
            {
                Debug.LogWarning($"表情 '{expression}' 无法从角色 '{name}'中找到");
                return;
            }

            // 在指定层过渡到新精灵
            TransitionSprite(sprite, layer);
        }
    }
}