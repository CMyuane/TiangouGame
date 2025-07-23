﻿using DIALOGUE;
using System.Collections.Generic;
using UnityEngine;

public class TestDialogueFiles : MonoBehaviour
{
    [SerializeField] private TextAsset fileToRead = null;

    // Start is called before the first frame update
    private void Start()
    {
        //string line = "Speaker \"Dialogue \\\"Goes In\\\" Here!\" Command(arguments here)";
        StartConversation();
        //DialogueParser.Parse(line);
    }

    // Update is called once per frame
    private void StartConversation()
    {
        List<string> lines = FileManager.ReadTextAsset(fileToRead);

        //测试解析对话行 command
        // foreach (string line in lines)
        // {
        //     if (string.IsNullOrWhiteSpace(line))
        //         continue;

        //     DIALOGUE_LINE dl = DialogueParser.Parse(line);

        //     for(int i = 0; i < dl.commandsData.commands.Count; i++)
        //     {
        //         DL_COMAND_DATA.Command command = dl.commandsData.commands[i];
        //         Debug.Log($"Command [{i}] '{command.name}' has arguments [{string.Join(", ", command.arguments)}]");
        //     }
        // }

        //测试解析对话行 speaker
        // for(int i = 0; i < lines.Count; i++)
        // {
        //     string line = lines[i];

        //     if (string.IsNullOrEmpty(line))
        //         continue;

        //     DIALOGUE_LINE dl = DialogueParser.Parse(line);

        //     Debug.Log($"{dl.speaker.name} as [{(dl.speaker.castName != string.Empty ? dl.speaker.castName : dl.speaker.name)}]at {dl.speaker.castPosition}");

        //     List<(int l,string ex)> expr = dl.speaker.CastExpressions;
        //     for(int c = 0; c < expr.Count; c++)
        //     {
        //         Debug.Log($"[Layer[{expr[c].l}] = '{expr[c].ex}']");
        //     }
        // }

        //测试解析对话行 dialogue
        // foreach (string line in lines)
        // {
        //     if (string.IsNullOrEmpty(line))
        //         continue;
        //     Debug.Log($"Segmenting line: '{line}'");
        //     DIALOGUE_LINE dlLine = DialogueParser.Parse(line);

        //     int i = 0;
        //     foreach(DL_DIALOGUE_DATA.DIALOGUE_SEGMENT segment in dlLine.dialogue.segments)
        //     {
        //         Debug.Log($"Segment [{i++}] = '{segment.dialogue}' [signal={segment.startSignal.ToString()}{(segment.signalDelay > 0? $"{segment.signalDelay}":$"")}]");
        //     }
        // }

        DialogueSystem.instance.Say(lines);
    }
}