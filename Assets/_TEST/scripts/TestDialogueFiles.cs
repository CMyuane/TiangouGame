using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueFiles : MonoBehaviour
{
    // Start is called before the first frame update  
    void Start()
    {
        //string line = "Speaker \"Dialogue \\\"Goes In\\\" Here!\" Command(arguments here)";
        StartConversation();
        //DialogueParser.Parse(line);
    }

    // Update is called once per frame
    void StartConversation()
    {
        List<string> lines = FileManager.ReadTextAsset("testFile");

        DialogueSystem.instance.Say(lines);
    }
}

