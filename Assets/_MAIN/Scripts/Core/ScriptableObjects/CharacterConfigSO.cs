using UnityEngine;

namespace CHARACTERS
{
    [CreateAssetMenu(fileName = "Character Configration Asset", menuName = "Dialogue System/Character Configuration Asset")]
    public class CharacterConfigSO : ScriptableObject
    {
        public CharacterConfigData[] character;

        public CharacterConfigData GetConfig(string characterName)
        {
            characterName = characterName.ToLower();

            for (int i = 0; i < character.Length; i++)
            {
                CharacterConfigData data = character[i];

                if (string.Equals(characterName, data.name.ToLower()) || string.Equals(characterName, data.alias.ToLower()))
                    return data.Copy();
            }

            return CharacterConfigData.Default;
        }
    }
}