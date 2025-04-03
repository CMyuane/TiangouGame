using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


//人物名字框，隶属于对话框
namespace DIALOGUE
{
    [System.Serializable]
    public class NameContainer
    {
        [SerializeField] private GameObject root;           //姓名子框根对象
        [SerializeField] private TextMeshProUGUI nameText;  //姓名文本组件

        //显示姓名框
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
    }
}