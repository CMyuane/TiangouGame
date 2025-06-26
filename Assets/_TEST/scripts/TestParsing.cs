using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TESTING
{
    public class TestParsing : MonoBehaviour
    {
        //首帧更新前调用start Start is called before the first frame update  
        void Start()
        {
            //string line = "Speaker \"Dialogue \\\"Goes In\\\" Here!\" Command(arguments here)";
            SendFileToParse();
            //DialogueParser.Parse(line);
        }

        //每帧调用一次更新 Update is called once per frame
        void SendFileToParse()
        {
            //从文件管理器获取测试文件内容
            List<string> lines = FileManager.ReadTextAsset("testFile");
            foreach(string line in lines)
            {
                if (line == string.Empty)
                    continue;
                //对话解析器处理每行内容
                DIALOGUE_LINE dl = DialogueParser.Parse(line);
            }
        }
    }
}