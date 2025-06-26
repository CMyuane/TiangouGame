using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DIALOGUE
{
    public class DIALOGUE_LINE
    {
        //Ëµ»°ÕßĞÕÃû¡¢¶Ô»°ÄÚÈİ¡¢ÄÚÇ¶ÃEE
        /// <summary>
        // ±úæ¾Ò»ĞĞ¶Ô»°µÄÀà£¬°E¬Ëµ»°Õß¡¢¶Ô»°ÄÚÈİºÍÃEû½£
        /// </summary>
        public DL_SPEAKER_DATA  speakerData;
        public DL_DIALOGUE_DATA dialogueData;
        public DL_COMAND_DATA commandsData;
        public string speaker => speakerData?.displayName ?? "";
        public string dialogue => dialogueData != null ? string.Concat(dialogueData.segments.ConvertAll(s => s.dialogue)) : "";
        public string commands => commandsData != null ? string.Join(", ", commandsData.commands.ConvertAll(c => c.name)) : "";
        public List<DL_COMAND_DATA.Command> commandList => commandsData?.commands;
        // ÊÇ·ñÓĞËµ»°ÕßĞÕÃE
        public bool hasSpeaker => speakerData != null;//speaker != string.Empty;
        // ÊÇ·ñÓĞ¶Ô»°ÄÚÈİ
        public bool hasDialogue =>dialogueData != null;//dialogue != string.Empty;
        // ÊÇ·ñÓĞÃEE
        public bool hasCommands => commandsData != null;

        /// <summary>
        /// ¹¹ÔE¯Êı£¬³õÊ¼»¯¶Ô»°ĞĞ¡£
        /// </summary>
        /// <param name="speaker">Ëµ»°ÕßĞÕÃE/param>
        /// <param name="dialogue">¶Ô»°ÄÚÈİ</param>
        /// <param name="commands">ÄÚÇ¶ÃEE/param>
        public DIALOGUE_LINE(string speaker, string dialogue, string commands)
        {
            this.speakerData = (string.IsNullOrWhiteSpace(speaker) ? null : new DL_SPEAKER_DATA(speaker));
            this.dialogueData = (string.IsNullOrWhiteSpace(dialogue) ? null : new DL_DIALOGUE_DATA(dialogue));
            this.commandsData = (string.IsNullOrWhiteSpace(commands) ? null : new DL_COMAND_DATA(commands));
        }
    }
}