using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DIALOGUE
{
    public class DIALOGUE_LINE
    {
        //˵�����������Ի����ݡ���Ƕ����
        /// <summary>
        // ��ʾһ�жԻ����࣬����˵���ߡ��Ի����ݺ����
        /// </summary>
        public DL_SPEAKER_DATA  speakerData;
        public DL_DIALOGUE_DATA dialogueData;
        public DL_COMAND_DATA commandsData;

        // �Ƿ���˵��������
        public bool hasSpeaker => speakerData != null;//speaker != string.Empty;
        // �Ƿ��жԻ�����
        public bool hasDialogue =>dialogueData != null;//dialogue != string.Empty;
        // �Ƿ�������
        public bool hasCommands => commandsData != null;

        /// <summary>
        /// ���캯������ʼ���Ի��С�
        /// </summary>
        /// <param name="speaker">˵��������</param>
        /// <param name="dialogue">�Ի�����</param>
        /// <param name="commands">��Ƕ����</param>
        public DIALOGUE_LINE(string speaker, string dialogue, string commands)
        {
            this.speakerData = (string.IsNullOrWhiteSpace(speaker) ? null : new DL_SPEAKER_DATA(speaker));
            this.dialogueData = (string.IsNullOrWhiteSpace(dialogue) ? null : new DL_DIALOGUE_DATA(dialogue));
            this.commandsData = (string.IsNullOrWhiteSpace(commands) ? null : new DL_COMAND_DATA(commands));
        }
    }
}