using UnityEngine;
using TMPro;
using System.Collections;

    /// <summary>
    /// �Ի������࣬�����Ի������� UI Ԫ�ء�
    /// </summary>
namespace DIALOGUE
{
    //���౾�������л�������Щ������ʹ����������ڼ��������ʾ������
    [System.Serializable]
    public class DialogueContainer
    {
        //������Ϸ���� ����root�����ضԻ���
        public GameObject root;
        public NameContainer nameContainer;     //�����ı���������������ʾ˵��������
        public TextMeshProUGUI dialogueText;    //�Ի��ı����Ի���������ʾ�Ի�����

        public void SetDialogueColor(Color color) => dialogueText.color = color;
        public void SetDialogueFont(TMP_FontAsset font) => dialogueText.font = font;
    }
}