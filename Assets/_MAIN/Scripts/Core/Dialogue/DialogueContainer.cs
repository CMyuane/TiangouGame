using UnityEngine;
using TMPro;
using System.Collections;

//对话框（对话容器)
namespace DIALOGUE
{
    //在类本身中序列化所有这些变量，使其参数可以在检查器中显示并操作
    [System.Serializable]
    public class DialogueContainer
    {
        //公共游戏对象 禁用root会隐藏对话框
        public GameObject root;
        public NameContainer nameContainer;     //名字文本（名字容器）
        public TextMeshProUGUI dialogueText;    //对话文本（对话容器）
    }
}