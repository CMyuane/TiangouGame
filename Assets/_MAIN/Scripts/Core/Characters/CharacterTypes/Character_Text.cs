using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ÕâÊÇÒ»¸ö½ÇÉ«ÎÄ±¾Àà£¬¼Ì³Ğ×Ô½ÇÉ«Àà¡£ËEúæ¾ÓÎÏ·ÖĞµÄÎÄ±¾½ÇÉ«£¬²¢ÔÚ´´½¨Ê±Êä³öµ÷ÊÔĞÅÏ¢¡£
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