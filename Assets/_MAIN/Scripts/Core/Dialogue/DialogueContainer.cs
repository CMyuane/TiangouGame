using UnityEngine;
using TMPro;
using System.Collections;

//�Ի��򣨶Ի�����)
namespace DIALOGUE
{
    //���౾�������л�������Щ������ʹ����������ڼ��������ʾ������
    [System.Serializable]
    public class DialogueContainer
    {
        //������Ϸ���� ����root�����ضԻ���
        public GameObject root;
        public NameContainer nameContainer;     //�����ı�������������
        public TextMeshProUGUI dialogueText;    //�Ի��ı����Ի�������
    }
}