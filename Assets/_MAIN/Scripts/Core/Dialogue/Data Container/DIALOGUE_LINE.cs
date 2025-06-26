using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DIALOGUE
{
    public class DIALOGUE_LINE
    {
        //˵�����������Ի����ݡ���ǶÁE�E
        /// <summary>
        // ���һ�жԻ����࣬��E�˵���ߡ��Ի����ݺ�ÁE���
        /// </summary>
        public DL_SPEAKER_DATA  speakerData;
        public DL_DIALOGUE_DATA dialogueData;
        public DL_COMAND_DATA commandsData;
        public string speaker => speakerData?.displayName ?? "";
        public string dialogue => dialogueData != null ? string.Concat(dialogueData.segments.ConvertAll(s => s.dialogue)) : "";
        public string commands => commandsData != null ? string.Join(", ", commandsData.commands.ConvertAll(c => c.name)) : "";
        public List<DL_COMAND_DATA.Command> commandList => commandsData?.commands;
        // �Ƿ���˵������ÁE
        public bool hasSpeaker => speakerData != null;//speaker != string.Empty;
        // �Ƿ��жԻ�����
        public bool hasDialogue =>dialogueData != null;//dialogue != string.Empty;
        // �Ƿ���ÁE�E
        public bool hasCommands => commandsData != null;

        /// <summary>
        /// ��ԁE�������ʼ���Ի��С�
        /// </summary>
        /// <param name="speaker">˵������ÁE/param>
        /// <param name="dialogue">�Ի�����</param>
        /// <param name="commands">��ǶÁE�E/param>
        public DIALOGUE_LINE(string speaker, string dialogue, string commands)
        {
            this.speakerData = (string.IsNullOrWhiteSpace(speaker) ? null : new DL_SPEAKER_DATA(speaker));
            this.dialogueData = (string.IsNullOrWhiteSpace(dialogue) ? null : new DL_DIALOGUE_DATA(dialogue));
            this.commandsData = (string.IsNullOrWhiteSpace(commands) ? null : new DL_COMAND_DATA(commands));
        }
    }
}