using DIALOGUE;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 角色管理器：负责游戏中所有角色的创建、管理和配置
namespace CHARACTERS
{
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager instance { get; private set; } // 单例实例
        public Character[] allCharacters => characters.Values.ToArray(); // 获取所有角色数组
        private Dictionary<string, Character> characters = new Dictionary<string, Character>(); // 角色字典（小写名->角色对象）

        private CharacterConfigSO config => DialogueSystem.instance.config.characterConfigurationAsset; // 角色配置资源

        // 角色信息解析常量
        private const string CHARACTER_CASTING_ID = " as "; // 角色别名标识符

        private const string CHARACTER_NAME_ID = "<charname>"; // 角色名占位符

        // 资源路径格式
        public string characterRootPathFormat => $"Characters/{CHARACTER_NAME_ID}"; // 角色根路径格式

        public string characterPrefabNameFormat => $"Character-[{CHARACTER_NAME_ID}]"; // 角色预制体名称格式
        public string characterPrefabPathFormat => $"{characterRootPathFormat}/{characterPrefabNameFormat}"; // 完整预制体路径格式

        // 角色面板引用
        [SerializeField] private RectTransform _characterpannel = null; // 通用角色面板

        [SerializeField] private RectTransform _characterpannel_live2D = null; // Live2D角色面板
        [SerializeField] private Transform _characterpannel_model3D = null; // 3D模型角色面板
        public RectTransform characterPanel => _characterpannel; // 通用角色面板访问器
        public RectTransform characterPanelLive2D => _characterpannel_live2D; // Live2D面板访问器
        public Transform characterPanelModel3D => _characterpannel_model3D; // 3D模型面板访问器

        private void Awake()
        {
            instance = this; // 初始化单例
        }

        // 获取角色配置数据
        public CharacterConfigData GetCharacterConfig(string characterName)
        {
            return config.GetConfig(characterName);
        }

        // 获取角色（如果不存在可创建）
        public Character GetCharacter(string characterName, bool createIfDoesNotExist = false)
        {
            if (characters.ContainsKey(characterName.ToLower()))
                return characters[characterName.ToLower()];
            else if (createIfDoesNotExist)
                return CreateCharacter(characterName); // 创建新角色

            return null; // 角色不存在且不创建
        }

        // 检查角色是否存在
        public bool HasCharacter(string characterName) => characters.ContainsKey(characterName.ToLower());

        // 创建新角色
        public Character CreateCharacter(string characterName, bool revealAfterCreation = false)
        {
            if (characters.ContainsKey(characterName.ToLower()))
            {
                Debug.LogWarning($"角色 '{characterName}' 已存在，不再创建");
                return null;
            }

            // 解析角色信息
            CHARACTER_INFO info = GetCharacterInfo(characterName);

            // 根据信息创建角色
            Character character = CreateCharacterFromInfo(info);

            characters.Add(info.name.ToLower(), character);

            // 如果需要，创建后立即显示
            if (revealAfterCreation)
                character.Show();

            return character;
        }

        // 解析角色信息
        private CHARACTER_INFO GetCharacterInfo(string characterName)
        {
            CHARACTER_INFO result = new CHARACTER_INFO();

            // 分割角色名和别名（格式："角色名 as 别名"）
            string[] nameData = characterName.Split(CHARACTER_CASTING_ID, System.StringSplitOptions.RemoveEmptyEntries);
            result.name = nameData[0]; // 实际使用的名称
            result.castingName = nameData.Length > 1 ? nameData[1] : result.name; // 配置使用的别名

            // 获取角色配置
            result.config = config.GetConfig(result.castingName);

            // 获取角色预制体
            result.prefab = GetPrefabForCharacter(result.castingName);

            // 格式化角色根路径
            result.rootCharacterFolder = FormatCharacterPath(characterRootPathFormat, result.castingName);

            return result;
        }

        // 获取角色预制体
        private GameObject GetPrefabForCharacter(string characterName)
        {
            string prefabPath = FormatCharacterPath(characterPrefabPathFormat, characterName);
            return Resources.Load<GameObject>(prefabPath);
        }

        // 格式化角色路径（替换占位符）
        public string FormatCharacterPath(string path, string characterName) =>
            path.Replace(CHARACTER_NAME_ID, characterName);

        // 根据角色信息创建具体类型的角色
        private Character CreateCharacterFromInfo(CHARACTER_INFO info)
        {
            CharacterConfigData config = info.config;

            // 根据角色类型创建不同的角色实例
            switch (config.characterType)
            {
                case Character.CharacterType.Text:
                    return new Character_Text(info.name, config); // 纯文本角色

                case Character.CharacterType.Sprite:
                case Character.CharacterType.SpriteSheet:
                    return new Character_Sprite(info.name, config, info.prefab, info.rootCharacterFolder); // 精灵角色

                //case Character.CharacterType.Live2D:
                //    return new Character_Live2D(info.name, config, info.prefab, info.rootCharacterFolder); // Live2D角色

                //case Character.CharacterType.Model3D:
                //    return new Character_Model3D(info.name, config, info.prefab, info.rootCharacterFolder); // 3D模型角色

                default:
                    return null; // 未知类型
            }
        }

        // 对所有角色进行排序
        public void SortCharacters()
        {
            // 获取活动中的可见角色
            List<Character> activeCharacters = characters.Values
                .Where(c => c.root.gameObject.activeInHierarchy && c.isVisible)
                .ToList();

            // 获取非活动角色
            List<Character> inactiveCharacters = characters.Values
                .Except(activeCharacters)
                .ToList();

            // 按优先级排序活动角色
            activeCharacters.Sort((a, b) => a.priority.CompareTo(b.priority));
            activeCharacters.Concat(inactiveCharacters);

            SortCharacters(activeCharacters);
        }

        // 按指定名称顺序排序角色
        public void SortCharacters(string[] characterNames)
        {
            List<Character> sortedCharacters = new List<Character>();

            sortedCharacters = characterNames
                .Select(name => GetCharacter(name))
                .Where(character => character != null)
                .ToList();

            // 获取剩余角色并按优先级排序
            List<Character> remainingCharacters = characters.Values
                .Except(sortedCharacters)
                .OrderBy(character => character.priority)
                .ToList();

            // 反转排序角色列表（使第一个成为最高优先级）
            sortedCharacters.Reverse();

            // 计算起始优先级
            int startingPriority = remainingCharacters.Count > 0 ?
                remainingCharacters.Max(c => c.priority) : 0;

            // 为排序角色设置新优先级
            for (int i = 0; i < sortedCharacters.Count; i++)
            {
                Character character = sortedCharacters[i];
                character.SetPriority(startingPriority + i + 1, autoSortCharactersOnUI: false);
            }

            // 合并所有角色（剩余角色 + 排序角色）
            List<Character> allCharacters = remainingCharacters.Concat(sortedCharacters).ToList();

            // 应用排序
            SortCharacters(allCharacters);
        }

        // 应用排序到UI层级
        private void SortCharacters(List<Character> charactersSortingOrder)
        {
            int i = 0;
            foreach (Character character in charactersSortingOrder)
            {
                Debug.Log($"{character.name} 优先级为 {character.priority}");
                character.root.SetSiblingIndex(i++); // 设置UI层级顺序
                character.OnSort(i); // 通知角色排序事件
            }
        }

        // 统计指定类型的角色数量
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

        // 角色信息内部类
        private class CHARACTER_INFO
        {
            public string name = ""; // 角色实际名称
            public string castingName = ""; // 角色配置名称（别名）
            public string rootCharacterFolder = ""; // 角色资源根路径
            public CharacterConfigData config = null; // 角色配置数据
            public GameObject prefab = null; // 角色预制体
        }
    }
}