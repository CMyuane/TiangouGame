using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;


    /// <summary>
    /// 名字容器类，用于显示和隐藏说话者名字。
    /// </summary>人物名字框，隶属于对话框
namespace DIALOGUE
{
    [System.Serializable]
    public class NameContainer
    {
        [SerializeField] private GameObject root;           //姓名子框根对象
        [SerializeField] private TextMeshProUGUI nameText;  //姓名文本组件

        /// <summary>
        /// 显示名字框，并可选更新名字。
        /// </summary>
        /// <param name="nameToShow">要显示的名字</param>
        public void Show(string nameToShow = "")
        {
            root.SetActive(true);

            //仅当传入非空姓名时更新文本 ，允许不更新姓名只显示框体，空姓名处理需要实际业务逻辑确认
            if (nameToShow != string.Empty)
                nameText.text = nameToShow;
        }

        //隐藏姓名框
        public void Hide()
        {
            root.SetActive(false);
        }
        public void SetNameColor(Color color) => nameText.color = color;
        public void SetNameFont(TMP_FontAsset font) => nameText.font = font;
    }
}