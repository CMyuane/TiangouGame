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
        // 参数别名定义（英文/中文）
        private static string[] PARAM_ENABLE => new string[] { "-e", "-enabled", "-启用" };        // 是否启用角色
        private static string[] PARAM_IMMEDIATE => new string[] { "-i", "-immediate", "-立即" };   // 是否立即执行
        private static string[] PARAM_SPEED => new string[] { "-spd", "-speed", "-速度" };         // 动画速度
        private static string[] PARAM_DURATION => new string[] { "-dat", "-duration", "-时间" };   // 持续时间（暂未实现）
        private static string[] PARAM_SMOOTH => new string[] { "-sm", "-smooth", "-平滑" };        // 是否平滑过渡
        private static string[] PARAM_XPOS => new string[] { "-x", "-xpos", "-横" };              // X轴位置
        private static string[] PARAM_YPOS => new string[] { "-y", "-ypos", "-纵" };              // Y轴位置

        // 扩展命令数据库
        new public static void Extend(CommandDatabase database)
        {
            // 添加全局角色命令
            database.AddCommand("createcharacter", new Action<string[]>(CreateCharacter));       // 创建角色
            database.AddCommand("movecharacter", new Func<string[], IEnumerator>(MoveCharacter)); // 移动角色
            database.AddCommand("show", new Func<string[], IEnumerator>(ShowAll));                // 显示所有指定角色
            database.AddCommand("hide", new Func<string[], IEnumerator>(HideAll));                // 隐藏所有指定角色
            database.AddCommand("sort", new Action<string[]>(Sort));                             // 角色排序
            database.AddCommand("highlight", new Func<string[], IEnumerator>(HighlightAll));      // 高亮指定角色
            database.AddCommand("unhighlight", new Func<string[], IEnumerator>(UnhighlightAll));  // 取消高亮指定角色

            // 创建基础角色命令子数据库
            CommandDatabase baseCommands = CommandManager.instance.CreateSubDatabase(CommandManager.DATABASE_CHARACTERS_BASE);
            baseCommands.AddCommand("move", new Func<string[], IEnumerator>(MoveCharacter));      // 移动角色
            baseCommands.AddCommand("show", new Func<string[], IEnumerator>(Show));               // 显示单个角色
            baseCommands.AddCommand("hide", new Func<string[], IEnumerator>(Hide));               // 隐藏单个角色
            baseCommands.AddCommand("setpriority", new Action<string[]>(SetPriority));            // 设置渲染优先级
            baseCommands.AddCommand("setposition", new Action<string[]>(SetPosition));            // 设置角色位置
            baseCommands.AddCommand("setColor", new Func<string[], IEnumerator>(SetColor));       // 设置角色颜色
            baseCommands.AddCommand("highlight", new Func<string[], IEnumerator>(Highlight));     // 高亮单个角色
            baseCommands.AddCommand("unhighlight", new Func<string[], IEnumerator>(Unhighlight)); // 取消高亮单个角色

            // 创建精灵角色命令子数据库
            CommandDatabase spriteCommands = CommandManager.instance.CreateSubDatabase(CommandManager.DATABASE_CHARACTERS_SPRITE);
            spriteCommands.AddCommand("setsprite", new Func<string[], IEnumerator>(SetSprite));   // 设置精灵图片
        }

        #region 全局命令
        // 创建角色命令
        private static void CreateCharacter(string[] data)
        {
            string characterName = data[0];  // 第一个参数为角色名
            bool enable = false;             // 是否启用角色
            bool immediate = false;          // 是否立即显示

            // 解析参数
            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(PARAM_ENABLE, out enable, defaultValue: false);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            // 创建角色
            Character character = CharacterManager.instance.CreateCharacter(characterName);

            // 如果启用角色，则显示
            if (enable)
            {
                if (immediate)
                    character.isVisible = true;  // 立即显示
                else
                    character.Show();           // 动画显示
            }
        }

        // 角色排序命令
        private static void Sort(string[] data)
        {
            CharacterManager.instance.SortCharacters(data);  // 调用角色管理器排序
        }

        // 移动角色命令（协程）
        private static IEnumerator MoveCharacter(string[] data)
        {
            string characterName = data[0];  // 第一个参数为角色名
            Character character = CharacterManager.instance.GetCharacter(characterName);

            if (character == null) yield break;  // 角色不存在则退出

            // 初始化移动参数
            float x = 0, y = 0;
            float speed = 1;
            bool smooth = false;
            bool immediate = false;

            // 解析参数
            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(PARAM_XPOS, out x);                     // 获取X坐标
            parameters.TryGetValue(PARAM_YPOS, out y);                     // 获取Y坐标
            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1); // 移动速度
            parameters.TryGetValue(PARAM_SMOOTH, out smooth, defaultValue: false); // 是否平滑移动
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false); // 是否立即移动

            Vector2 position = new Vector2(x, y);  // 目标位置

            // 执行移动
            if (immediate)
                character.SetPosition(position);  // 立即设置位置
            else
            {
                // 添加终止回调（强制完成移动）
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => {
                    character?.SetPosition(position);
                });

                // 执行移动动画
                yield return character.MoveToPosition(position, speed, smooth);
            }
        }

        // 显示多个角色命令（协程）
        private static IEnumerator ShowAll(string[] data)
        {
            List<Character> characters = new List<Character>();  // 要显示的角色列表
            bool immediate = false;   // 是否立即显示
            float speed = 1;          // 显示速度

            // 获取所有指定角色
            foreach (string name in data)
            {
                Character character = CharacterManager.instance.GetCharacter(name, createIfDoesNotExist: false);
                if (character != null) characters.Add(character);
            }

            if (characters.Count == 0)
            {
                Debug.LogWarning("没有可显示的角色");
                yield break;
            }

            // 解析参数
            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);

            // 显示所有角色
            foreach (Character character in characters)
            {
                if (immediate)
                    character.isVisible = true;  // 立即显示
                else
                    character.Show(speed);       // 动画显示
            }

            // 等待所有动画完成
            if (!immediate)
            {
                // 添加终止回调（强制完成显示）
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (Character character in characters)
                        character.isVisible = true;
                });

                // 等待所有角色完成显示动画
                while (characters.Any(c => c.isRevealing))
                    yield return null;
            }
        }

        // 隐藏多个角色命令（协程）
        private static IEnumerator HideAll(string[] data)
        {
            List<Character> characters = new List<Character>();  // 要隐藏的角色列表
            bool immediate = false;   // 是否立即隐藏
            float speed = 1f;         // 隐藏速度

            // 获取所有指定角色
            foreach (string name in data)
            {
                Character character = CharacterManager.instance.GetCharacter(name, createIfDoesNotExist: false);
                if (character != null) characters.Add(character);
            }

            if (characters.Count == 0)
            {
                Debug.LogWarning("没有可隐藏的角色");
                yield break;
            }

            // 解析参数
            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);

            // 隐藏所有角色
            foreach (Character character in characters)
            {
                if (immediate)
                    character.isVisible = false;  // 立即隐藏
                else
                    character.Hide(speed);        // 动画隐藏
            }

            // 等待所有动画完成
            if (!immediate)
            {
                // 添加终止回调（强制完成隐藏）
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (Character character in characters)
                        character.isVisible = false;
                });

                // 等待所有角色完成隐藏动画
                while (characters.Any(c => c.isHiding))
                    yield return null;
            }
        }

        // 高亮多个角色命令（协程）
        public static IEnumerator HighlightAll(string[] data)
        {
            List<Character> characters = new List<Character>();  // 要高亮的角色
            bool immediate = false;  // 是否立即执行
            bool handleUnspecifiedCharacters = true;  // 是否处理未指定角色
            List<Character> unspecifiedCharacters = new List<Character>();  // 未指定角色列表

            // 添加要高亮的角色
            for (int i = 0; i < data.Length; i++)
            {
                Character character = CharacterManager.instance.GetCharacter(data[i], createIfDoesNotExist: false);
                if (character != null) characters.Add(character);
            }

            if (characters.Count == 0) yield break;

            // 解析参数
            var parameters = ConvertDataToParameters(data, startingIndex: 1);
            parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: false);
            parameters.TryGetValue(new string[] { "-o", "-only" }, out handleUnspecifiedCharacters, defaultValue: true);

            // 高亮指定角色
            foreach (Character character in characters)
                character.Highlight(immediate: immediate);

            // 处理未指定角色（取消高亮）
            if (handleUnspecifiedCharacters)
            {
                foreach (Character character in CharacterManager.instance.allCharacters)
                {
                    if (characters.Contains(character)) continue;
                    unspecifiedCharacters.Add(character);
                    character.UnHighlight(immediate: immediate);
                }
            }

            // 等待动画完成
            if (!immediate)
            {
                // 添加终止回调（强制完成高亮）
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (var character in characters)
                        character.Highlight(immediate: true);

                    if (!handleUnspecifiedCharacters) return;

                    foreach (var character in unspecifiedCharacters)
                        character.UnHighlight(immediate: true);
                });

                // 等待所有高亮/取消高亮动画完成
                while (characters.Any(c => c.isHighlighting) ||
                      (handleUnspecifiedCharacters && unspecifiedCharacters.Any(uc => uc.isUnHighlighting)))
                    yield return null;
            }
        }

        // 取消高亮多个角色命令（协程）
        public static IEnumerator UnhighlightAll(string[] data)
        {
            List<Character> characters = new List<Character>();  // 要取消高亮的角色
            bool immediate = false;  // 是否立即执行
            bool handleUnspecifiedCharacters = true;  // 是否处理未指定角色
            List<Character> unspecifiedCharacters = new List<Character>();  // 未指定角色列表

            // 添加要取消高亮的角色
            for (int i = 0; i < data.Length; i++)
            {
                Character character = CharacterManager.instance.GetCharacter(data[i], createIfDoesNotExist: false);
                if (character != null) characters.Add(character);
            }

            if (characters.Count == 0) yield break;

            // 解析参数
            var parameters = ConvertDataToParameters(data, startingIndex: 1);
            parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: false);
            parameters.TryGetValue(new string[] { "-o", "-only" }, out handleUnspecifiedCharacters, defaultValue: true);

            // 取消高亮指定角色
            foreach (Character character in characters)
                character.UnHighlight(immediate: immediate);

            // 处理未指定角色（恢复高亮）
            if (handleUnspecifiedCharacters)
            {
                foreach (Character character in CharacterManager.instance.allCharacters)
                {
                    if (characters.Contains(character)) continue;
                    unspecifiedCharacters.Add(character);
                    character.Highlight(immediate: immediate);
                }
            }

            // 等待动画完成
            if (!immediate)
            {
                // 添加终止回调（强制完成取消高亮）
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (var character in characters)
                        character.UnHighlight(immediate: true);

                    if (!handleUnspecifiedCharacters) return;

                    foreach (var character in unspecifiedCharacters)
                        character.Highlight(immediate: true);
                });

                // 等待所有取消高亮/高亮动画完成
                while (characters.Any(c => c.isUnHighlighting) ||
                      (handleUnspecifiedCharacters && unspecifiedCharacters.Any(uc => uc.isHighlighting)))
                    yield return null;
            }
        }
        #endregion

        #region 基础角色命令
        // 显示单个角色命令（协程）
        private static IEnumerator Show(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0]);  // 获取角色
            if (character == null) yield break;

            bool immediate = false;
            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: false);

            if (immediate)
                character.isVisible = true;  // 立即显示
            else
            {
                // 添加终止回调（强制显示）
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => {
                    if (character != null) character.isVisible = true;
                });

                // 执行显示动画
                yield return character.Show();
            }
        }

        // 隐藏单个角色命令（协程）
        private static IEnumerator Hide(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0]);  // 获取角色
            if (character == null) yield break;

            bool immediate = false;
            var parameters = ConvertDataToParameters(data);
            parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: false);

            if (immediate)
                character.isVisible = false;  // 立即隐藏
            else
            {
                // 添加终止回调（强制隐藏）
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => {
                    if (character != null) character.isVisible = false;
                });

                // 执行隐藏动画
                yield return character.Hide();
            }
        }

        // 设置角色位置命令
        public static void SetPosition(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false);
            if (character == null || data.Length < 2) return;

            float x = 0, y = 0;
            var parameters = ConvertDataToParameters(data, 1);

            // 获取坐标参数
            parameters.TryGetValue(PARAM_XPOS, out x, defaultValue: 0);
            parameters.TryGetValue(PARAM_YPOS, out y, defaultValue: 0);

            // 设置位置
            character.SetPosition(new Vector2(x, y));
        }

        // 设置渲染优先级命令
        public static void SetPriority(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false);
            if (character == null || data.Length < 2) return;

            int priority;
            // 解析优先级数值
            if (!int.TryParse(data[1], out priority)) priority = 0;

            // 设置优先级
            character.SetPriority(priority);
        }

        // 设置角色颜色命令（协程）
        public static IEnumerator SetColor(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false);
            if (character == null || data.Length < 2) yield break;

            string colorName;  // 颜色名称
            float speed;      // 过渡速度
            bool immediate;   // 是否立即执行

            // 解析参数
            var parameters = ConvertDataToParameters(data, startingIndex: 1);
            parameters.TryGetValue(new string[] { "-c", "-color" }, out colorName);  // 颜色名
            bool specifiedSpeed = parameters.TryGetValue(new string[] { "-spd", "-speed" }, out speed, defaultValue: 1f);  // 速度

            // 确定是否立即执行（未指定速度则立即执行）
            if (!specifiedSpeed)
                parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: true);
            else
                immediate = false;

            // 通过名称获取颜色
            Color color = Color.white.GetColorFromName(colorName);

            if (immediate)
                character.SetColor(color);  // 立即设置颜色
            else
            {
                // 添加终止回调（强制设置颜色）
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => {
                    character?.SetColor(color);
                });

                // 执行颜色过渡动画
                character.TransitionColor(color, speed);
            }
            yield break;
        }

        // 高亮单个角色命令（协程）
        public static IEnumerator Highlight(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false);
            if (character == null) yield break;

            bool immediate = false;
            var parameters = ConvertDataToParameters(data, startingIndex: 1);
            parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: false);

            if (immediate)
                character.Highlight(immediate: true);  // 立即高亮
            else
            {
                // 添加终止回调（强制高亮）
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => {
                    character?.Highlight(immediate: true);
                });

                // 执行高亮动画
                yield return character.Highlight();
            }
        }

        // 取消高亮单个角色命令（协程）
        public static IEnumerator Unhighlight(string[] data)
        {
            Character character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false);
            if (character == null) yield break;

            bool immediate = false;
            var parameters = ConvertDataToParameters(data, startingIndex: 1);
            parameters.TryGetValue(new string[] { "-i", "-immediate" }, out immediate, defaultValue: false);

            if (immediate)
                character.UnHighlight(immediate: true);  // 立即取消高亮
            else
            {
                // 添加终止回调（强制取消高亮）
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => {
                    character?.UnHighlight(immediate: true);
                });

                // 执行取消高亮动画
                yield return character.UnHighlight();
            }
        }
        #endregion

        #region 精灵角色命令
        // 设置精灵图片命令（协程）
        public static IEnumerator SetSprite(string[] data)
        {
            // 获取精灵角色
            Character_Sprite character = CharacterManager.instance.GetCharacter(data[0], createIfDoesNotExist: false) as Character_Sprite;
            if (character == null || data.Length < 2) yield break;

            int layer = 0;         // 精灵层
            string spriteName;      // 精灵名称
            bool immediate = false; // 是否立即切换
            float speed;            // 切换速度

            // 解析参数
            var parameters = ConvertDataToParameters(data, startingIndex: 1);
            parameters.TryGetValue(new string[] { "-s", "-sprite" }, out spriteName);  // 精灵名
            parameters.TryGetValue(new string[] { "-l", "-layer" }, out layer, defaultValue: 0);  // 层级

            // 获取速度参数并确定是否立即执行
            bool specifiedSpeed = parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 0.1f);
            if (!specifiedSpeed)
                parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: true);

            // 获取精灵对象
            Sprite sprite = character.GetSprite(spriteName);
            if (sprite == null) yield break;

            // 设置精灵
            if (immediate)
            {
                character.SetSprite(sprite, layer);  // 立即设置
            }
            else
            {
                // 添加终止回调（强制设置精灵）
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => {
                    character?.SetSprite(sprite, layer);
                });

                // 执行精灵过渡动画
                yield return character.TransitionSprite(sprite, layer, speed);
            }
        }
        #endregion
    }
}