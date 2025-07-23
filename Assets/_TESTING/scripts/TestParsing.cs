using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TESTING
{
    public class TestParsing : MonoBehaviour
    {
        //��֡����ǰ����start Start is called before the first frame update  
        void Start()
        {
            //string line = "Speaker \"Dialogue \\\"Goes In\\\" Here!\" Command(arguments here)";
            SendFileToParse();
            //DialogueParser.Parse(line);
        }

        //ÿ֡����һ�θ��� Update is called once per frame
        void SendFileToParse()
        {
            //���ļ���������ȡ�����ļ�����
            List<string> lines = FileManager.ReadTextAsset("testFile");
            foreach(string line in lines)
            {
                if (line == string.Empty)
                    continue;
                //�Ի�����������ÿ������
                DIALOGUE_LINE dl = DialogueParser.Parse(line);
            }
        }
    }
}