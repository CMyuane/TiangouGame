using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//����һ����ɫ�ı��࣬�̳��Խ�ɫ�ࡣˁE����Ϸ�е��ı���ɫ�����ڴ���ʱ���������Ϣ��
namespace CHARACTERS
{
    public class Character_Text : Character
    {
        public Character_Text(string name, CharacterConfigData config) : base(name, config, prefab:null)
        {
            Debug.Log($"Creating Text character: '{name}'");
        }
    }
}