using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DIALOGUE
{
    public class DIALOGUE_LINE
    {
        //说话者姓名、对话内容、内嵌命令
        /// <summary>
        // 表示一行对话的类，包含说话者、对话内容和命令。
        /// </summary>
        public DL_SPEAKER_DATA  speakerData;
        public DL_DIALOGUE_DATA dialogueData;
        public DL_COMAND_DATA commandsData;

        // 是否有说话者姓名
        public bool hasSpeaker => speakerData != null;//speaker != string.Empty;
        // 是否有对话内容
        public bool hasDialogue =>dialogueData != null;//dialogue != string.Empty;
        // 是否有命令
        public bool hasCommands => commandsData != null;

        /// <summary>
        /// 构造函数，初始化对话行。
        /// </summary>
        /// <param name="speaker">说话者姓名</param>
        /// <param name="dialogue">对话内容</param>
        /// <param name="commands">内嵌命令</param>
        public DIALOGUE_LINE(string speaker, string dialogue, string commands)
        {
            this.speakerData = (string.IsNullOrWhiteSpace(speaker) ? null : new DL_SPEAKER_DATA(speaker));
            this.dialogueData = (string.IsNullOrWhiteSpace(dialogue) ? null : new DL_DIALOGUE_DATA(dialogue));
            this.commandsData = (string.IsNullOrWhiteSpace(commands) ? null : new DL_COMAND_DATA(commands));
        }
    }
}