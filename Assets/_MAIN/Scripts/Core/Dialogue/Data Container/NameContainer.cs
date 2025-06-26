using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;


    /// <summary>
    /// ���������࣬������ʾ������˵�������֡�
    /// </summary>�������ֿ������ڶԻ���
namespace DIALOGUE
{
    [System.Serializable]
    public class NameContainer
    {
        [SerializeField] private GameObject root;           //�����ӿ������
        [SerializeField] private TextMeshProUGUI nameText;  //�����ı����

        /// <summary>
        /// ��ʾ���ֿ򣬲���ѡ�������֡�
        /// </summary>
        /// <param name="nameToShow">Ҫ��ʾ������</param>
        public void Show(string nameToShow = "")
        {
            root.SetActive(true);

            //��������ǿ�����ʱ�����ı� ����������������ֻ��ʾ���壬������������Ҫʵ��ҵ���߼�ȷ��
            if (nameToShow != string.Empty)
                nameText.text = nameToShow;
        }

        //����������
        public void Hide()
        {
            root.SetActive(false);
        }
        public void SetNameColor(Color color) => nameText.color = color;
        public void SetNameFont(TMP_FontAsset font) => nameText.font = font;
    }
}