using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DIALOGUE;
using System.Linq;

// ��ɫ��������������Ϸ�����н�ɫ�Ĵ��������������
namespace CHARACTERS
{
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager instance { get; private set; } // ����ʵ��
        public Character[] allCharacters => characters.Values.ToArray(); // ��ȡ���н�ɫ����
        private Dictionary<string, Character> characters = new Dictionary<string, Character>(); // ��ɫ�ֵ䣨Сд��->��ɫ����

        private CharacterConfigSO config => DialogueSystem.instance.config.characterConfigurationAsset; // ��ɫ������Դ

        // ��ɫ��Ϣ��������
        private const string CHARACTER_CASTING_ID = " as "; // ��ɫ������ʶ��
        private const string CHARACTER_NAME_ID = "<charname>"; // ��ɫ��ռλ��

        // ��Դ·����ʽ
        public string characterRootPathFormat => $"Characters/{CHARACTER_NAME_ID}"; // ��ɫ��·����ʽ
        public string characterPrefabNameFormat => $"Character-[{CHARACTER_NAME_ID}]"; // ��ɫԤ�������Ƹ�ʽ
        public string characterPrefabPathFormat => $"{characterRootPathFormat}/{characterPrefabNameFormat}"; // ����Ԥ����·����ʽ

        // ��ɫ�������
        [SerializeField] private RectTransform _characterpannel = null; // ͨ�ý�ɫ���
        [SerializeField] private RectTransform _characterpannel_live2D = null; // Live2D��ɫ���
        [SerializeField] private RectTransform _characterpannel_model3D = null; // 3Dģ�ͽ�ɫ���
        public RectTransform characterPanel => _characterpannel; // ͨ�ý�ɫ��������
        public RectTransform characterPanelLive2D => _characterpannel_live2D; // Live2D��������
        public RectTransform characterPanelModel3D => _characterpannel_model3D; // 3Dģ����������

        private void Awake()
        {
            instance = this; // ��ʼ������
        }

        // ��ȡ��ɫ��������
        public CharacterConfigData GetCharacterConfig(string characterName)
        {
            return config.GetConfig(characterName);
        }

        // ��ȡ��ɫ����������ڿɴ�����
        public Character GetCharacter(string characterName, bool createIfDoesNotExist = false)
        {
            string key = characterName.ToLower();
            if (characters.ContainsKey(key))
                return characters[key]; // �������н�ɫ
            else if (createIfDoesNotExist)
                return CreateCharacter(characterName); // �����½�ɫ

            return null; // ��ɫ�������Ҳ�����
        }

        // ����ɫ�Ƿ����
        public bool HasCharacter(string characterName) => characters.ContainsKey(characterName.ToLower());

        // �����½�ɫ
        public Character CreateCharacter(string characterName, bool revealAfterCreation = false)
        {
            string key = characterName.ToLower();
            if (characters.ContainsKey(key))
            {
                Debug.LogWarning($"��ɫ '{characterName}' �Ѵ��ڣ����ٴ���");
                return characters[key];
            }

            // ������ɫ��Ϣ
            CHARACTER_INFO info = GetCharacterInfo(characterName);

            // ������Ϣ������ɫ
            Character character = CreateCharacterFromInfo(info);

            // ��ӵ��ֵ�
            characters.Add(key, character);

            // �����Ҫ��������������ʾ
            if (revealAfterCreation)
                character.Show();

            return character;
        }

        // ������ɫ��Ϣ
        private CHARACTER_INFO GetCharacterInfo(string characterName)
        {
            CHARACTER_INFO result = new CHARACTER_INFO();

            // �ָ��ɫ���ͱ�������ʽ��"��ɫ�� as ����"��
            string[] nameData = characterName.Split(CHARACTER_CASTING_ID, System.StringSplitOptions.RemoveEmptyEntries);
            result.name = nameData[0]; // ʵ��ʹ�õ�����
            result.castingName = nameData.Length > 1 ? nameData[1] : result.name; // ����ʹ�õı���

            // ��ȡ��ɫ����
            result.config = config.GetConfig(result.castingName);

            // ��ȡ��ɫԤ����
            result.prefab = GetPrefabForCharacter(result.castingName);

            // ��ʽ����ɫ��·��
            result.rootCharacterFolder = FormatCharacterPath(characterRootPathFormat, result.castingName);

            return result;
        }

        // ��ȡ��ɫԤ����
        private GameObject GetPrefabForCharacter(string characterName)
        {
            string prefabPath = FormatCharacterPath(characterPrefabPathFormat, characterName);
            return Resources.Load<GameObject>(prefabPath);
        }

        // ��ʽ����ɫ·�����滻ռλ����
        public string FormatCharacterPath(string path, string characterName) =>
            path.Replace(CHARACTER_NAME_ID, characterName);

        // ���ݽ�ɫ��Ϣ�����������͵Ľ�ɫ
        private Character CreateCharacterFromInfo(CHARACTER_INFO info)
        {
            CharacterConfigData config = info.config;

            // ���ݽ�ɫ���ʹ�����ͬ�Ľ�ɫʵ��
            switch (config.characterType)
            {
                case Character.CharacterType.Text:
                    return new Character_Text(info.name, config); // ���ı���ɫ

                case Character.CharacterType.Sprite:
                case Character.CharacterType.SpriteSheet:
                    return new Character_Sprite(info.name, config, info.prefab, info.rootCharacterFolder); // �����ɫ

                default:
                    return null; // δ֪����
            }
        }

        // �����н�ɫ��������
        public void SortCharacters()
        {
            // ��ȡ��еĿɼ���ɫ
            List<Character> activeCharacters = characters.Values
                .Where(c => c.root.gameObject.activeInHierarchy && c.isVisible)
                .ToList();

            // ��ȡ�ǻ��ɫ
            List<Character> inactiveCharacters = characters.Values
                .Except(activeCharacters)
                .ToList();

            // �����ȼ�������ɫ
            activeCharacters.Sort((a, b) => a.priority.CompareTo(b.priority));

            // �ϲ��б����ɫ��ǰ��
            List<Character> allCharacters = activeCharacters.Concat(inactiveCharacters).ToList();

            // Ӧ������
            SortCharacters(allCharacters);
        }

        // ��ָ������˳�������ɫ
        public void SortCharacters(string[] characterNames)
        {
            // ������˳���ȡ��ɫ
            List<Character> sortedCharacters = characterNames
                .Select(name => GetCharacter(name))
                .Where(character => character != null)
                .ToList();

            // ��ȡʣ���ɫ�������ȼ�����
            List<Character> remainingCharacters = characters.Values
                .Except(sortedCharacters)
                .OrderBy(character => character.priority)
                .ToList();

            // ��ת�����ɫ�б�ʹ��һ����Ϊ������ȼ���
            sortedCharacters.Reverse();

            // ������ʼ���ȼ�
            int startingPriority = remainingCharacters.Count > 0 ?
                remainingCharacters.Max(c => c.priority) : 0;

            // Ϊ�����ɫ���������ȼ�
            for (int i = 0; i < sortedCharacters.Count; i++)
            {
                Character character = sortedCharacters[i];
                character.SetPriority(startingPriority + i + 1, autoSortCharactersOnUI: false);
            }

            // �ϲ����н�ɫ��ʣ���ɫ + �����ɫ��
            List<Character> allCharacters = remainingCharacters.Concat(sortedCharacters).ToList();

            // Ӧ������
            SortCharacters(allCharacters);
        }

        // Ӧ������UI�㼶
        private void SortCharacters(List<Character> charactersSortingOrder)
        {
            int i = 0;
            foreach (Character character in charactersSortingOrder)
            {
                Debug.Log($"{character.name} ���ȼ�Ϊ {character.priority}");
                character.root.SetSiblingIndex(i++); // ����UI�㼶˳��
                character.OnSort(i); // ֪ͨ��ɫ�����¼�
            }
        }

        // ͳ��ָ�����͵Ľ�ɫ����
        public int GetCharacterCountFromCharacterType(Character.CharacterType charType)
        {
            int count = 0;
            foreach (var c in characters.Values)
            {
                if (c.config.characterType == charType)
                    count++;
            }
            return count;
        }

        // ��ɫ��Ϣ�ڲ���
        private class CHARACTER_INFO
        {
            public string name = ""; // ��ɫʵ������
            public string castingName = ""; // ��ɫ�������ƣ�������
            public string rootCharacterFolder = ""; // ��ɫ��Դ��·��
            public CharacterConfigData config = null; // ��ɫ��������
            public GameObject prefab = null; // ��ɫԤ����
        }
    }
}