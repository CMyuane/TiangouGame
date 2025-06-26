using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CHARACTERS;
using System.Linq;
using UnityEditor;

namespace COMMANDS
{
    public class CMD_DatabaseExtension_Characters : CMD_DatabaseExtension
    {
        // �����������壨Ӣ��/���ģ�
        private static string[] PARAM_ENABLE => new string[] { "-e", "-enabled", "-����" };        // �Ƿ����ý�ɫ
        private static string[] PARAM_IMMEDIATE => new string[] { "-i", "-immediate", "-����" };   // �Ƿ�����ִ��
        private static string[] PARAM_SPEED => new string[] { "-spd", "-speed", "-�ٶ�" };         // �����ٶ�
        private static string[] PARAM_DURATION => new string[] { "-dat", "-duration", "-ʱ��" };   // ����ʱ�䣨��δʵ�֣�
        private static string[] PARAM_SMOOTH => new string[] { "-sm", "-smooth", "-ƽ��" };        // �Ƿ�ƽ������
        private static string[] PARAM_XPOS => new string[] { "-x", "-xpos", "-��" };              // X��λ��
        private static string[] PARAM_YPOS => new string[] { "-y", "-ypos", "-��" };              // Y��λ��

        // ��չ�������ݿ�
        new public static void Extend(CommandDatabase database)
        {
            // ���ȫ�ֽ�ɫ����
            database.AddCommand("createcharacter", new Action<string[]>(CreateCharacter));       // ������ɫ
            database.AddCommand("movecharacter", new Func<string[], IEnumerator>(MoveCharacter)); // �ƶ���ɫ
            database.AddCommand("show", new Func<string[], IEnumerator>(ShowAll));                // ��ʾ����ָ����ɫ
            database.AddCommand("hide", new Func<string[], IEnumerator>(HideAll));                // ��������ָ����ɫ
            database.AddCommand("sort", new Action<string[]>(Sort));                             // ��ɫ����
            database.AddCommand("highlight", new Func<string[], IEnumerator>(HighlightAll));      // ����ָ����ɫ
            database.AddCommand("unhighlight", new Func<string[], IEnumerator>(UnhighlightAll));  // ȡ������ָ����ɫ

            // ����������ɫ���������ݿ�
            CommandDatabase baseCommands = CommandManager.instance.CreateSubDatabase(CommandManager.DATABASE_CHARACTERS_BASE);
            baseCommands.AddCommand("move", new Func<string[], IEnumerator>(MoveCharacter));      // �ƶ���ɫ
            baseCommands.AddCommand("show", new Func<string[], IEnumerator>(Show));               // ��ʾ������ɫ
            baseCommands.AddCommand("hide", new Func<string[], IEnumerator>(Hide));               // ���ص�����ɫ
            baseCommands.AddCommand("setpriority", new Action<string[]>(SetPriority));            // ������Ⱦ���ȼ�
            baseCommands.AddCommand("setposition", new Action<string[]>(SetPosition));            // ���ý�ɫλ��
            baseCommands.AddCommand("setColor", new Func<string[], IEnumerator>(SetColor));       // ���ý�ɫ��ɫ
            baseCommands.AddCommand("highlight", new Func<string[], IEnumerator>(Highlight));     // ����������ɫ
            baseCommands.AddCommand("unhighlight", new Func<string[], IEnumerator>(Unhighlight)); // ȡ������������ɫ

            // ���������ɫ���������ݿ�
            CommandDatabase spriteCommands = CommandManager.instance.CreateSubDatabase(CommandManager.DATABASE_CHARACTERS_SPRITE);
            spriteCommands.AddCommand("setsprite", new Func<string[], IEnumerator>(SetSprite));   // ���þ���ͼƬ
        }

        #region ȫ������
        // ������ɫ����
        private static void CreateCharacter(string[] data)
        {
            string characterName = data[0];  // ��һ������Ϊ��ɫ��
            bool enable = false;             // �Ƿ����ý�ɫ
            bool immediate = false;          // �Ƿ�������ʾ

            // ��������
            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(PARAM_ENABLE, out enable, defaultValue: false);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            // ������ɫ
            Character character = CharacterManager.instance.CreateCharacter(characterName);

            // ������ý�ɫ������ʾ
            if (enable)
            {
                if (immediate)
                    character.isVisible = true;  // ������ʾ
                else
                    character.Show();           // ������ʾ
            }
        }

        // ��ɫ��������
        private static void Sort(string[] data)
        {
            CharacterManager.instance.SortCharacters(data);  // ���ý�ɫ����������
        }

        // �ƶ���ɫ���Э�̣�
        private static IEnumerator MoveCharacter(string[] data)
        {
            string characterName = data[0];  // ��һ������Ϊ��ɫ��
            Character character = CharacterManager.instance.GetCharacter(characterName);

            if (character == null) yield break;  // ��ɫ���������˳�

            // ��ʼ���ƶ�����
            float x = 0, y = 0;
            float speed = 1;
            bool smooth = false;
            bool immediate = false;

            // ��������
            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(PARAM_XPOS, out x);                     // ��ȡX����
            parameters.TryGetValue(PARAM_YPOS, out y);                     // ��ȡY����
            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1); // �ƶ��ٶ�
            parameters.TryGetValue(PARAM_SMOOTH, out smooth, defaultValue: false); // �Ƿ�ƽ���ƶ�
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false); // �Ƿ������ƶ�

            Vector2 position = new Vector2(x, y);  // Ŀ��λ��

            // ִ���ƶ�
            if (immediate)
                character.SetPosition(position);  // ��������λ��
            else
            {
                // �����ֹ�ص���ǿ������ƶ���
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => {
                    character?.SetPosition(position);
                });

                // ִ���ƶ�����
                yield return character.MoveToPosition(position, speed, smooth);
            }
        }

        // ��ʾ�����ɫ���Э�̣�
        private static IEnumerator ShowAll(string[] data)
        {
            List<Character> characters = new List<Character>();  // Ҫ��ʾ�Ľ�ɫ�б�
            bool immediate = false;   // �Ƿ�������ʾ
            float speed = 1;          // ��ʾ�ٶ�

            // ��ȡ����ָ����ɫ
            foreach (string name in data)
            {
                Character character = CharacterManager.instance.GetCharacter(name, createIfDoesNotExist: false);
                if (character != null) characters.Add(character);
            }

            if (characters.Count == 0)
            {
                Debug.LogWarning("û�п���ʾ�Ľ�ɫ");
                yield break;
            }

            // ��������
            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);

            // ��ʾ���н�ɫ
            foreach (Character character in characters)
            {
                if (immediate)
                    character.isVisible = true;  // ������ʾ
                else
                    character.Show(speed);       // ������ʾ
            }

            // �ȴ����ж������
            if (!immediate)
            {
                // �����ֹ�ص���ǿ�������ʾ��
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (Character character in characters)
                        character.isVisible = true;
                });

                // �ȴ����н�ɫ�����ʾ����
                while (characters.Any(c => c.isRevealing))
                    yield return null;
            }
        }

        // ���ض����ɫ���Э�̣�
        private static IEnumerator HideAll(string[] data)
        {
            List<Character> characters = new List<Character>();  // Ҫ���صĽ�ɫ�б�
            bool immediate = false;   // �Ƿ���������
            float speed = 1f;         // �����ٶ�

            // ��ȡ����ָ����ɫ
            foreach (string name in data)
            {
                Character character = CharacterManager.instance.GetCharacter(name, createIfDoesNotExist: false);
                if (character != null) characters.Add(character);
            }

            if (characters.Count == 0)
            {
                Debug.LogWarning("û�п����صĽ�ɫ");
                yield break;
            }

            // ��������
            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);

            // �������н�ɫ
            foreach (Character character in characters)
            {
                if (immediate)
                    character.isVisible = false;  // ��������
                else
                    character.Hide(speed);        // ��������
            }

            // �ȴ����ж������
            if (!immediate)
            {
                // �����ֹ�ص���ǿ��������أ�
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (Character character in characters)
                        character.isVisible = false;
                });

                // �ȴ����н�ɫ������ض���
                while (characters.Any(c => c.isHiding))
                    yield return null;
            }
        }

        // ���������ɫ���Э�̣�
        public static IEnumerator HighlightAll(string[] data)
        {
            List<Character> characters = new List<Character>();  // Ҫ�����Ľ�ɫ
            bool immediate = false;  // �Ƿ�����ִ��
            bool handleUnspecifiedCharacters = true;  // �Ƿ���δָ����ɫ
            List<Character> unspecifiedCharacters = new List<Character>();  // δָ����ɫ�б�

            // ���Ҫ�����Ľ�ɫ
            for (int i = 0; i < data.Length; i++)
            {
                Character character = CharacterManager.instance.GetCharacter(data[i], createIfDoesNotExist: false);
                if (character != null) characters.Add(character);
            }

            if (characters.Count == 0) yield break;

            // ��������
            var parameters = ConvertDataToParameters(data, startingIndex: 1);
            parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: false);
            parameters.TryGetValue(new string[] { "-o", "-only" }, out handleUnspecifiedCharacters, defaultValue: true);

            // ����ָ����ɫ
            foreach (Character character in characters)
                character.Highlight(immediate: immediate);

            // ����δָ����ɫ��ȡ��������
            if (handleUnspecifiedCharacters)
            {
                foreach (Character character in CharacterManager.instance.allCharacters)
                {
                    if (characters.Contains(character)) continue;
                    unspecifiedCharacters.Add(character);
                    character.UnHighlight(immediate: immediate);
                }
            }

            // �ȴ��������
            if (!immediate)
            {
                // �����ֹ�ص���ǿ����ɸ�����
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (var character in characters)
                        character.Highlight(immediate: true);

                    if (!handleUnspecifiedCharacters) return;

                    foreach (var character in unspecifiedCharacters)
                        character.UnHighlight(immediate: true);
                });

                // �ȴ����и���/ȡ�������������
                while (characters.Any(c => c.isHighlighting) ||
                      (handleUnspecifiedCharacters && unspecifiedCharacters.Any(uc => uc.isUnHighlighting)))
                    yield return null;
            }
        }

        // ȡ�����������ɫ���Э�̣�
        public static IEnumerator UnhighlightAll(string[] data)
        {
            List<Character> characters = new List<Character>();  // Ҫȡ�������Ľ�ɫ
            bool immediate = false;  // �Ƿ�����ִ��
            bool handleUnspecifiedCharacters = true;  // �Ƿ���δָ����ɫ
            List<Character> unspecifiedCharacters = new List<Character>();  // δָ����ɫ�б�

            // ���Ҫȡ�������Ľ�ɫ
            for (int i = 0; i < data.Length; i++)
            {
                Character character = CharacterManager.instance.GetCharacter(data[i], createIfDoesNotExist: false);
                if (character != null) characters.Add(character);
            }

            if (characters.Count == 0) yield break;

            // ��������
            var parameters = ConvertDataToParameters(data, startingIndex: 1);
            parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: false);
            parameters.TryGetValue(new string[] { "-o", "-only" }, out handleUnspecifiedCharacters, defaultValue: true);

            // ȡ������ָ����ɫ
            foreach (Character character in characters)
                character.UnHighlight(immediate: immediate);

            // ����δָ����ɫ���ָ�������
            if (handleUnspecifiedCharacters)
            {
                foreach (Character character in CharacterManager.instance.allCharacters)
                {
                    if (characters.Contains(character)) continue;
                    unspecifiedCharacters.Add(character);
                    character.Highlight(immediate: immediate);
                }
            }

            // �ȴ��������
            if (!immediate)
            {
                // �����ֹ�ص���ǿ�����ȡ��������
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (var character in characters)
                        character.UnHighlight(immediate: true);

                    if (!handleUnspecifiedCharacters) return;

                    foreach (var character in unspecifiedCharacters)
                        character.Highlight(immediate: true);
                });

                // �ȴ�����ȡ������/�����������
                while (characters.Any(c => c.isUnHighlighting) ||
                      (handleUnspecifiedCharacters && unspecifiedCharacters.Any(uc => uc.isHighlighting)))
                    yield return null;
            }
        }
        #endregion

        #region ������ɫ����
        // ��ʾ������ɫ���Э�̣�
        private static IEnumerator Show(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0]);  // ��ȡ��ɫ
            if (character == null) yield break;

            bool immediate = false;
            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: false);

            if (immediate)
                character.isVisible = true;  // ������ʾ
            else
            {
                // �����ֹ�ص���ǿ����ʾ��
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => {
                    if (character != null) character.isVisible = true;
                });

                // ִ����ʾ����
                yield return character.Show();
            }
        }

        // ���ص�����ɫ���Э�̣�
        private static IEnumerator Hide(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0]);  // ��ȡ��ɫ
            if (character == null) yield break;

            bool immediate = false;
            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: false);

            if (immediate)
                character.isVisible = false;  // ��������
            else
            {
                // �����ֹ�ص���ǿ�����أ�
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => {
                    if (character != null) character.isVisible = false;
                });

                // ִ�����ض���
                yield return character.Hide();
            }
        }

        // ���ý�ɫλ������
        public static void SetPosition(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false);
            if (character == null || data.Length < 2) return;

            float x = 0, y = 0;
            var parameters = ConvertDataToParameters(data, 1);

            // ��ȡ�������
            parameters.TryGetValue(PARAM_XPOS, out x, defaultValue: 0);
            parameters.TryGetValue(PARAM_YPOS, out y, defaultValue: 0);

            // ����λ��
            character.SetPosition(new Vector2(x, y));
        }

        // ������Ⱦ���ȼ�����
        public static void SetPriority(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false);
            if (character == null || data.Length < 2) return;

            int priority;
            // �������ȼ���ֵ
            if (!int.TryParse(data[1], out priority)) priority = 0;

            // �������ȼ�
            character.SetPriority(priority);
        }

        // ���ý�ɫ��ɫ���Э�̣�
        public static IEnumerator SetColor(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false);
            if (character == null || data.Length < 2) yield break;

            string colorName;  // ��ɫ����
            float speed;      // �����ٶ�
            bool immediate;   // �Ƿ�����ִ��

            // ��������
            var parameters = ConvertDataToParameters(data, startingIndex: 1);
            parameters.TryGetValue(new string[] { "-c", "-color" }, out colorName);  // ��ɫ��
            bool specifiedSpeed = parameters.TryGetValue(new string[] { "-spd", "-speed" }, out speed, defaultValue: 1f);  // �ٶ�

            // ȷ���Ƿ�����ִ�У�δָ���ٶ�������ִ�У�
            if (!specifiedSpeed)
                parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: true);
            else
                immediate = false;

            // ͨ�����ƻ�ȡ��ɫ
            Color color = Color.white.GetColorFromName(colorName);

            if (immediate)
                character.SetColor(color);  // ����������ɫ
            else
            {
                // �����ֹ�ص���ǿ��������ɫ��
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => {
                    character?.SetColor(color);
                });

                // ִ����ɫ���ɶ���
                character.TransitionColor(color, speed);
            }
            yield break;
        }

        // ����������ɫ���Э�̣�
        public static IEnumerator Highlight(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false);
            if (character == null) yield break;

            bool immediate = false;
            var parameters = ConvertDataToParameters(data, startingIndex: 1);
            parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: false);

            if (immediate)
                character.Highlight(immediate: true);  // ��������
            else
            {
                // �����ֹ�ص���ǿ�Ƹ�����
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => {
                    character?.Highlight(immediate: true);
                });

                // ִ�и�������
                yield return character.Highlight();
            }
        }

        // ȡ������������ɫ���Э�̣�
        public static IEnumerator Unhighlight(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false);
            if (character == null) yield break;

            bool immediate = false;
            var parameters = ConvertDataToParameters(data, startingIndex: 1);
            parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: false);

            if (immediate)
                character.UnHighlight(immediate: true);  // ����ȡ������
            else
            {
                // �����ֹ�ص���ǿ��ȡ��������
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => {
                    character?.UnHighlight(immediate: true);
                });

                // ִ��ȡ����������
                yield return character.UnHighlight();
            }
        }
        #endregion

        #region �����ɫ����
        // ���þ���ͼƬ���Э�̣�
        public static IEnumerator SetSprite(string[] data)
        {
            // ��ȡ�����ɫ
            Character_Sprite character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false) as Character_Sprite;
            if (character == null || data.Length < 2) yield break;

            int layer = 0;         // �����
            string spriteName;      // ��������
            bool immediate = false; // �Ƿ������л�
            float speed;            // �л��ٶ�

            // ��������
            var parameters = ConvertDataToParameters(data, startingIndex: 1);
            parameters.TryGetValue(new string[] { "-s", "-sprite" }, out spriteName);  // ������
            parameters.TryGetValue(new string[] { "-l", "-layer" }, out layer, defaultValue: 0);  // �㼶

            // ��ȡ�ٶȲ�����ȷ���Ƿ�����ִ��
            bool specifiedSpeed = parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 0.1f);
            if (!specifiedSpeed)
                parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: true);

            // ��ȡ�������
            Sprite sprite = character.GetSprite(spriteName);
            if (sprite == null) yield break;

            // ���þ���
            if (immediate)
            {
                character.SetSprite(sprite, layer);  // ��������
            }
            else
            {
                // �����ֹ�ص���ǿ�����þ��飩
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => {
                    character?.SetSprite(sprite, layer);
                });

                // ִ�о�����ɶ���
                yield return character.TransitionSprite(sprite, layer, speed);
            }
        }
        #endregion
    }
}