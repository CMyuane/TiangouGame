using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// ��ɫ�����ࣺ�̳���Character���࣬�������ɫ����Ⱦ�Ͷ���
namespace CHARACTERS
{
    public class Character_Sprite : Character
    {
        // ��������
        private const string SPRITE_RENDERER_PARENT_NAME = "Renderers"; // ������Ⱦ������������
        private const string SPRITESHEET_DEFAULT_SHEETNAME = "Default"; // Ĭ�Ͼ��������
        private const char SPRITESHEET_TEX_SPRITE_DELIMITTER = '-'; // ����������뾫�����Ʒָ���

        // �������
        private CanvasGroup rootCG => root.GetComponent<CanvasGroup>(); // �������CanvasGroup���

        // ��������
        public List<CharacterSpriteLayer> layers = new List<CharacterSpriteLayer>(); // ������б�

        // ��Դ·��
        private string artAssetsDirectory = ""; // ������ԴĿ¼·��

        // ��ɫ�ɼ�������
        public override bool isVisible
        {
            get { return isRevealing || rootCG.alpha > 0; } // �ж��Ƿ�ɼ�
            set { rootCG.alpha = value ? 1 : 0; } // ����͸����
        }

        // ���캯��
        public Character_Sprite(string name, CharacterConfigData config, GameObject prefab, string rootAssetsFolder)
            : base(name, config, prefab) // ���û��๹�캯��
        {
            // ��ʼ����ɫ͸���ȣ������Ƿ����ã�
            rootCG.alpha = ENABLE_ON_START ? 1 : 0;

            // ����������ԴĿ¼·��
            artAssetsDirectory = rootAssetsFolder + "/Images";

            // ��ȡ����ʼ�������
            GetLayers();
        }

        // ��ȡ���о����
        private void GetLayers()
        {
            // ������Ⱦ��������
            Transform rendererRoot = animator.transform.Find(SPRITE_RENDERER_PARENT_NAME);
            if (rendererRoot == null) return;

            // ���������Ӷ���
            for (int i = 0; i < rendererRoot.transform.childCount; i++)
            {
                Transform child = rendererRoot.transform.GetChild(i);

                // ��ȡͼ�����
                Image rendererImage = child.GetComponentInChildren<Image>();

                if (rendererImage != null)
                {
                    // ��������㲢��ӵ��б�
                    CharacterSpriteLayer layer = new CharacterSpriteLayer(rendererImage, i);
                    layers.Add(layer);

                    // �������Ӷ����Ա����
                    child.name = $"Layer: {i}";
                }
            }
        }

        // ����ָ����ľ���
        public void SetSprite(Sprite sprite, int layer = 0)
        {
            layers[layer].SetSprite(sprite);
        }

        // ��ȡ������Դ
        public Sprite GetSprite(string spriteName)
        {
            // ����������ͽ�ɫ
            if (config.characterType == CharacterType.SpriteSheet)
            {
                // �ָ��������;�����
                string[] data = spriteName.Split(SPRITESHEET_TEX_SPRITE_DELIMITTER);
                Sprite[] spriteArray = new Sprite[0];

                if (data.Length == 2)
                {
                    // ��ȡ�������ƺ;�������
                    string textureName = data[0];
                    spriteName = data[1];

                    // ���ؾ����
                    spriteArray = Resources.LoadAll<Sprite>($"{artAssetsDirectory}/{textureName}");
                }
                else
                {
                    // ʹ��Ĭ�Ͼ����
                    spriteArray = Resources.LoadAll<Sprite>($"{artAssetsDirectory}/{SPRITESHEET_DEFAULT_SHEETNAME}");
                }

                // ����Ƿ���سɹ�
                if (spriteArray.Length == 0)
                    Debug.LogWarning($"��ɫ '{name}' û��Ĭ�ϵľ�����޷���ȡ���� '{SPRITESHEET_DEFAULT_SHEETNAME}'");

                // ����ƥ�����Ƶľ���
                return Array.Find(spriteArray, sprite => sprite.name == spriteName);
            }
            else // ��ͨ��������
            {
                // ֱ�Ӽ��ص�������
                return Resources.Load<Sprite>($"{artAssetsDirectory}/{spriteName}");
            }
        }

        // ������ɶ���
        public Coroutine TransitionSprite(Sprite sprite, int layer = 0, float speed = 1)
        {
            CharacterSpriteLayer spriteLayer = layers[layer];
            Debug.Log($"TransitionSprite called with layer: {layer}");
            return spriteLayer.TransitionSprite(sprite, speed);
        }

        // ��ʾ/���ؽ�ɫ��Э��
        public override IEnumerator ShowingOrHiding(bool show, float speedMultiplier = 1f)
        {
            // ����Ŀ��͸����
            float targetAlpha = show ? 1f : 0;
            CanvasGroup self = rootCG;

            // �������͸����
            while (self.alpha != targetAlpha)
            {
                self.alpha = Mathf.MoveTowards(self.alpha, targetAlpha,
                    3f * Time.deltaTime * speedMultiplier);
                yield return null;
            }

            // ����Э������
            co_revealing = null;
            co_hiding = null;
        }

        // ���ý�ɫ��ɫ
        public override void SetColor(Color color)
        {
            base.SetColor(color); // ���û��෽��

            // Ӧ�õ����о����
            foreach (CharacterSpriteLayer layer in layers)
            {
                layer.StopChangingColor(); // ֹͣ���ڽ��е���ɫ�仯
                layer.SetColor(displayColor); // ��������ɫ
            }
        }

        // ��ɫ�仯����
        public override IEnumerator ChangingColor(float speed)
        {
            // Ϊ���в�������ɫ����
            foreach (CharacterSpriteLayer layer in layers)
                layer.TransitionColor(displayColor, speed);

            yield return null;

            // �ȴ����в������ɫ�仯
            while (layers.Any(l => l.isChangingColor))
                yield return null;

            // ����Э������
            co_changingColor = null;
        }

        // ������ɫ����
        public override IEnumerator Highlighting(float speedMultiplier, bool immediate = false)
        {
            // Ӧ�õ����о����
            foreach (CharacterSpriteLayer layer in layers)
            {
                if (immediate)
                    layer.SetColor(displayColor); // ����������ɫ
                else
                    layer.TransitionColor(displayColor, speedMultiplier); // ����������ɫ
            }

            yield return null;

            // �ȴ����в������ɫ�仯
            while (layers.Any(l => l.isChangingColor))
                yield return null;

            // ����Э������
            co_highlighting = null;
        }

        // �ı��ɫ����
        public override IEnumerator FaceDirection(bool faceLeft, float speedMultiplier, bool immediate)
        {
            // Ϊ���в����ó���
            foreach (CharacterSpriteLayer layer in layers)
            {
                if (faceLeft)
                    layer.FaceLeft(speedMultiplier, immediate); // ����
                else
                    layer.FaceRight(speedMultiplier, immediate); // ����
            }

            yield return null;

            // �ȴ����в���ɷ�ת
            while (layers.Any(l => l.isFlipping))
                yield return null;

            // ����Э������
            co_flipping = null;
        }

        // �����ɫ����ָ��
        public override void OnReceiveCastingExpression(int layer, string expression)
        {
            // ��ȡ��Ӧ����ľ���
            Sprite sprite = GetSprite(expression);

            if (sprite == null)
            {
                Debug.LogWarning($"���� '{expression}' �޷��ӽ�ɫ '{name}'���ҵ�");
                return;
            }

            // ��ָ������ɵ��¾���
            TransitionSprite(sprite, layer);
        }
    }
}