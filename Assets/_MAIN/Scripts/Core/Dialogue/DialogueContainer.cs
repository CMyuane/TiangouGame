using TMPro;
using UnityEngine;

/// <summary>
/// 对话容器类，包含对话框和相关 UI 元素。
/// </summary>
namespace DIALOGUE
{
    //在类本身中序列化所有这些变量，使其参数可以在检查器中显示并操作
    [System.Serializable]
    public class DialogueContainer
    {
        //公共游戏对象 禁用root会隐藏对话框
        public GameObject root;

        public NameContainer nameContainer;     //名字文本（名字容器）显示说话者名字
        public TextMeshProUGUI dialogueText;    //对话文本（对话容器）显示对话内容

        public void SetDialogueColor(Color color) => dialogueText.color = color;

        public void SetDialogueFont(TMP_FontAsset font) => dialogueText.font = font;
    }
}