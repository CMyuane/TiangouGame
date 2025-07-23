using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//这是一个角色文本类，继承自角色类。它表示游戏中的文本角色，并在创建时输出调试信息。
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