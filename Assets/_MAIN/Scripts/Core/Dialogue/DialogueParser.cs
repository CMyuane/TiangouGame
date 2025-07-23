using System.Text.RegularExpressions;
using UnityEngine;

namespace DIALOGUE
{
    /// <summary>
    /// 对话解析器类，用于解析对话文件中的每一行内容。
    /// </summary>
    public class DialogueParser
    {
        // 正则表达式模式，用于匹配命令
        private const string commandRegexPattern = @"[\w\[\]]*[^\s]\(";

        /// <summary>
        /// 解析一行原始对话文本。
        /// </summary>
        /// <param name="rawLine">原始对话文本</param>
        /// <returns>解析后的 DIALOGUE_LINE 对象</returns>
        public static DIALOGUE_LINE Parse(string rawLine)
        {
            Debug.Log($"Parsing line - '{rawLine}'");

            // 提取说话者、对话内容和命令
            (string speaker, string dialogue, string commands) = RipContent(rawLine);

            // Debug.Log($"Speaker = '{speaker}'\nDialogue = '{dialogue}'\nCommands = '{commands}'");

            return new DIALOGUE_LINE(speaker, dialogue, commands);
        }

        /// <summary>
        /// 从原始文本中提取说话者、对话内容和命令。
        /// </summary>
        /// <param name="rawLine">原始对话文本</param>
        /// <returns>元组，包含说话者、对话内容和命令</returns>
        private static (string, string, string) RipContent(string rawLine)
        {
            string speaker = "", dialogue = "", commands = "";

            int dialogueStart = -1;
            int dialogueEnd = -1;
            bool isEscaped = false;

            // 遍历文本，查找对话的起始和结束位置
            for (int i = 0; i < rawLine.Length; i++)
            {
                char current = rawLine[i];
                if (current == '\\')
                    isEscaped = !isEscaped;
                else if (current == '"' && !isEscaped)
                {
                    if (dialogueStart == -1)
                        dialogueStart = i;
                    else if (dialogueEnd == -1)
                        dialogueEnd = i;
                }
                else
                    isEscaped = false;
            }

            //Debug.Log(rawLine.Substring(dialogueStart + 1, dialogueEnd - dialogueStart - 1));
            Regex commandRegex = new Regex(commandRegexPattern);
            MatchCollection matches = commandRegex.Matches(rawLine);
            int commandStart = -1;
            foreach (Match match in matches)
            {
                if (match.Index < dialogueStart || match.Index > dialogueEnd)
                {
                    commandStart = match.Index;
                    break;
                }
            }

            if (commandStart != -1 && (dialogueStart == -1 && dialogueEnd == -1))
            {
                return ("", "", rawLine.Trim());
            }

            // if (match.Success)
            // {
            //     commandStart = match.Index;

            //     if (dialogueStart == -1 && dialogueEnd == -1)
            //         return ("", "", rawLine.Trim());
            // }

            if (dialogueStart != -1 && dialogueEnd != -1 && (commandStart == -1 || commandStart > dialogueEnd))
            {
                speaker = rawLine.Substring(0, dialogueStart).Trim();
                dialogue = rawLine.Substring(dialogueStart + 1, dialogueEnd - dialogueStart - 1).Replace("\\\"", "\"");
                if (commandStart != -1)
                    commands = rawLine.Substring(commandStart).Trim();
            }
            else if (commandStart != -1 && dialogueStart > commandStart)
                commands = rawLine;
            else
                dialogue = rawLine;

            // Debug.Log($"DS = {dialogueStart}, DE = {dialogueEnd}, CS = {commandStart}");
            return (speaker, dialogue, commands);
        }
    }
}