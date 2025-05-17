using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//这个脚本定义了一个抽象的角色类，表示游戏中的角色。它包含了角色的基本属性和方法，用于处理角色的对话和文本样式等功能。
namespace CHARACTERS
{
    public abstract class Character
    {
        // 角色的基本配置数据
        public const bool ENABLE_ON_START = false; //创建时不可见
        private const float UNHIGHLIGHTED_DARKEN_STRENGTH = 0.5f;
        public const bool DEFAULT_ORIENTATION_IS_FACING_LEFT = true;
        public const string ANIMATION_REFRESH_TRIGGER = "Refresh";

        public string name = "";
        public string displayName = "";
        public RectTransform root = null;
        public CharacterConfigData config;
        public Animator animator;
        public Color color { get; protected set; } = Color.white;
        protected Color displayColor => highlighted ? highlightedColor : unhighlightedColor;
        protected Color highlightedColor => color;
        protected Color unhighlightedColor => new Color(color.r * UNHIGHLIGHTED_DARKEN_STRENGTH, color.g * UNHIGHLIGHTED_DARKEN_STRENGTH, color.b * UNHIGHLIGHTED_DARKEN_STRENGTH, color.a);
        public bool highlighted { get; protected set; } = true;
        protected bool facingLeft = DEFAULT_ORIENTATION_IS_FACING_LEFT;
        public int priority { get; protected set; }

        // 角色的基本属性
        protected CharacterManager characterManager => CharacterManager.instance;
        public DialogueSystem dialogueSystem => DialogueSystem.instance;

        protected Coroutine co_revealing, co_hiding;
        protected Coroutine co_moving;
        protected Coroutine co_changingColor;
        protected Coroutine co_highlighting;
        protected Coroutine co_flipping;
        public bool isRevealing => co_revealing != null;
        public bool isHiding => co_hiding != null;
        public bool isMoving => co_moving != null;
        public bool isChangingColor => co_changingColor != null;
        public bool isHighlighting => (highlighted && co_highlighting != null); 
        public bool isUnHighlighting => (!highlighted && co_highlighting != null); // 这个属性表示角色是否正在取消高亮显示
        public virtual bool isVisible  {get;set;}
        public bool isFacingLeft => facingLeft;
        public bool isFacingRight => !facingLeft;
        public bool isFlipping => co_flipping != null;

        public Character(string name, CharacterConfigData config, GameObject prefab)
        {
            this.name = name;
            displayName = name;
            this.config = config;

            if(prefab != null)
            {
                RectTransform parentPanel = null;
                switch (config.characterType)
                {
                    case CharacterType.Sprite:
                    case CharacterType.SpriteSheet:
                        parentPanel = characterManager.characterPanel;
                        break;
                }

                GameObject ob = Object.Instantiate(prefab, parentPanel);
                ob.name = characterManager.FormatCharacterPath(characterManager.characterPrefabNameFormat, name);
                ob.SetActive(true);
                root = ob.GetComponent<RectTransform>();
                animator = root.GetComponentInChildren<Animator>();
            }

        }

        public Coroutine Say(string dialogue) => Say(new List<string>() { dialogue });
        public Coroutine Say(List<string> dialogue)
        {
            dialogueSystem.ShowSpeakerName(displayName);
            UpdateTextCustomizationsOnScreen();
            return dialogueSystem.Say(dialogue);

        }

        public void SetNameFont(TMP_FontAsset font) => config.nameFont = font;
        public void SetDialogueFont(TMP_FontAsset font) => config.dialogueFont = font;
        public void SetNameColor(Color color) => config.nameColor = color;
        public void SetDialogueColor(Color color) => config.dialogueColor = color;

        public void ResetConfigurationData() => config = CharacterManager.instance.GetCharacterConfig(name);
        public void UpdateTextCustomizationsOnScreen() => dialogueSystem.ApplySpeakerDataToDialogueContainer(config);

        public virtual Coroutine Show()
        {
            if (isRevealing)
                return co_revealing;

            if(isHiding)
                characterManager.StopCoroutine(co_hiding);

            co_revealing = characterManager.StartCoroutine(ShowingOrHiding(true));

            return co_revealing;
        }

        public virtual Coroutine Hide()
        {
            if(isHiding)
                return co_hiding;

            if (isRevealing)
                characterManager.StopCoroutine(co_revealing);

            co_hiding = characterManager.StartCoroutine(ShowingOrHiding(false));

            return co_hiding;
        }

        public virtual IEnumerator ShowingOrHiding(bool show)
        {
            Debug.Log("Show/Hide cannot be called from a base character type.");
            yield return null;
        }

        public virtual void SetPosition(Vector2 position)
        {
            if (root == null)
                return;

            (Vector2 minAnchorTarget, Vector2 maxAnvhorTarget) = ConvertUITargetPositionToRelativeCharacterAnchorTargets(position);

            root.anchorMin = minAnchorTarget;
            root.anchorMax = maxAnvhorTarget;
        }

        public virtual Coroutine MoveToPosition(Vector2 position,float speed = 0.5f,bool smooth = false)
        {
            if (root == null)
                return null;

            if (isMoving)
                characterManager.StopCoroutine(co_moving);

            co_moving = characterManager.StartCoroutine(MovingToPosition(position, speed, smooth));

            return co_moving;
        }

        private IEnumerator MovingToPosition(Vector2 position, float duration, bool smooth)
        {
            (Vector2 minAnchorTarget, Vector2 maxAnchorTarget) = ConvertUITargetPositionToRelativeCharacterAnchorTargets(position);
            Vector2 padding = root.anchorMax - root.anchorMin;

            Vector2 startMin = root.anchorMin;
            Vector2 startMax = root.anchorMax;
            float elapsedTime = 0f;



            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);

                if (smooth)
                {
                    root.anchorMin = Vector2.Lerp(startMin, minAnchorTarget, t);
                }
                else
                {
                    // 非平滑模式下，使用匀速移动
                    root.anchorMin = Vector2.MoveTowards(
                        startMin,
                        minAnchorTarget,
                        Vector2.Distance(startMin, minAnchorTarget) * t
                    );
                }

                root.anchorMax = root.anchorMin + padding;

                yield return null;
            }

            // 确保最终位置精确
            root.anchorMin = minAnchorTarget;
            root.anchorMax = maxAnchorTarget;

            Debug.Log("Done moving");
            co_moving = null;
        }

        //private IEnumerator MovingToPosition(Vector2 position,float speed,bool smooth)
        //{
        //    (Vector2 minAnchorTarget, Vector2 maxAnvhorTarget) = ConvertUITargetPositionToRelativeCharacterAnchorTargets(position);
        //    Vector2 padding = root.anchorMax - root.anchorMin;

        //    while(root.anchorMin != minAnchorTarget || root.anchorMax != maxAnvhorTarget)
        //    {
        //        root.anchorMin = smooth ?
        //            Vector2.Lerp(root.anchorMin, minAnchorTarget, speed * Time.deltaTime)
        //            : Vector2.MoveTowards(root.anchorMin, minAnchorTarget, speed * Time.deltaTime * 0.35f);

        //        root.anchorMax = root.anchorMin + padding;

        //        if(smooth && Vector2.Distance(root.anchorMin,minAnchorTarget) <= 0.001f)
        //        {
        //            root.anchorMin = minAnchorTarget;
        //            root.anchorMax = maxAnvhorTarget;
        //            break;
        //        }
        //        yield return null;
        //    }

        //Debug.Log("Done moving");
        //    co_moving = null;
        //}

        protected(Vector2,Vector2) ConvertUITargetPositionToRelativeCharacterAnchorTargets(Vector2 position)
        {
            Vector2 padding = root.anchorMax - root.anchorMin;

            float maxX = 1f - padding.x;
            float maxY = 1f - padding.y;

            Vector2 minAnchorTarget= new Vector2(maxX*position.x, maxY * position.y);
            Vector2 maxAnchorTarget = minAnchorTarget + padding;

            return(minAnchorTarget,maxAnchorTarget);
        }

        public virtual void SetColor(Color color)
        {
            this.color = color;
            // 这里可以添加对角色颜色的设置逻辑
            // 例如，使用材质或其他方式来改变角色的颜色
        }

        public Coroutine TransitionColor(Color color, float speed = 1f)
        {
            this.color = color;

            if(isChangingColor)
                characterManager.StopCoroutine(co_changingColor);

            co_changingColor = characterManager.StartCoroutine(ChangingColor(displayColor, speed));

            return co_changingColor;
        }

        public virtual IEnumerator ChangingColor(Color color,float speed)
        {
            Debug.Log("该角色类型不支持颜色渐变。");
            yield return null;
        }

        public Coroutine Highlight(float speed = 1f)
        {
            if (isHighlighting)
                return co_highlighting;

            if (isUnHighlighting)
                characterManager.StopCoroutine(co_highlighting);

            highlighted = true;
            co_highlighting = characterManager.StartCoroutine(Highlighting(highlighted,speed));

            return co_highlighting;
        }

        public Coroutine UnHighlight(float speed = 1f)
        {
            if (isUnHighlighting)
                return co_highlighting;

            if (isHighlighting)
                characterManager.StopCoroutine(co_highlighting);

            highlighted = false;
            co_highlighting = characterManager.StartCoroutine(Highlighting(highlighted, speed));

            return co_highlighting;
        }

        public virtual IEnumerator Highlighting(bool highlighting,float speedMultiplier)
        {
            Debug.Log("该角色类型不支持高亮显示。");
            yield return null;
        }

        public Coroutine Flip(float speed = 1, bool immediate = false)
        {
            if (isFacingLeft)
                return FaceRight(speed, immediate);
            else
                return FaceLeft(speed, immediate);
        }

        public Coroutine FaceLeft(float speed = 1,bool immediate = false)
        {
            if (isFlipping)
                return co_flipping;

            facingLeft = true;
            co_flipping = characterManager.StartCoroutine(FaceDirection(facingLeft, speed, immediate));

            return co_flipping;
        }
        public Coroutine FaceRight(float speed = 1, bool immediate = false)
        {
            if (isFlipping)
                return co_flipping;

            facingLeft = false;
            co_flipping = characterManager.StartCoroutine(FaceDirection(facingLeft, speed, immediate));

            return co_flipping;
        }

        public virtual IEnumerator FaceDirection(bool faceLeft,float speedMultiplier,bool immediate)
        {
            Debug.Log("该角色类型不支持翻转。");
            yield return null;
        }

        public void SetPriority(int priority,bool autoSortCharactersOnUI = true)
        {
            this.priority = priority;

            if(autoSortCharactersOnUI)
                characterManager.SortCharacters();
        }

        public void Animate(string animation)
        {
            animator.SetTrigger(animation);
        }

        public void Animate(string animation,bool state)
        {
            animator.SetBool(animation, state);
            animator.SetTrigger(ANIMATION_REFRESH_TRIGGER);
        }

        public virtual void OnSort(int sortingIndex)
        {
            return;
        }

        public virtual void OnReceiveCastingExpression(int layer, string expression)
        {
            return;
        }

        public enum CharacterType
        {
            Text,
            Sprite,
            SpriteSheet,
        }

    }
}