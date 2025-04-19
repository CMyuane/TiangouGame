using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class DL_SPEAKER_DATA
{
    public string name, castName;//说话者姓名
    /// <summary>
    /// 显示姓名
    /// <para>如果说话者姓名不为空，则显示说话者姓名，否则显示原始姓名</para>
    /// </summary>
    public string displayName => (castName != string.Empty ? castName : name);//显示姓名
    public Vector2 castPosition;//说话者位置
    public List<(int Layer,string expression)> CastExpressions{ get; set;}//说话者表情

    private const string NAMECAST_ID = " as ";
    private const string POSITIONCAST_ID = " at ";
    private const string EXPRESSIONCAST_ID = " [";
    private const char AXISDELIMITER = ':';
    private const char EXPERSSIONLAYER_JOINER = ',';
    private const char EXPERSSIONLAYER_DELIMITER = ':';

    public DL_SPEAKER_DATA(string rawSpeaker)
    {
        string pattern = @$"{NAMECAST_ID}|{POSITIONCAST_ID}|{EXPRESSIONCAST_ID.Insert(EXPRESSIONCAST_ID.Length - 1, @"\")}";
        MatchCollection matches = Regex.Matches(rawSpeaker, pattern);

        //填充数据避免空引用
        castName = "";
        castPosition = Vector2.zero;
        CastExpressions = new List<(int Layer, string expression)>();

        //如果没有匹配到任何内容，直接返回原始说话者名称
        if (matches.Count == 0)
        {
            name = rawSpeaker;
            return;
        }

        int Index = matches[0].Index;
        name = rawSpeaker.Substring(0, Index);

        //
        for(int i = 0; i < matches.Count ; i++)
        {
            Match match = matches[i];
            int startIndex = 0 , endIndex = 0;

            if(match.Value == NAMECAST_ID)
            {  
                startIndex = match.Index + NAMECAST_ID.Length;
                endIndex = (i < matches.Count - 1) ? matches[i + 1].Index : rawSpeaker.Length;
                castName = rawSpeaker.Substring(startIndex, endIndex - startIndex);
            }
            else if(match.Value == POSITIONCAST_ID)
            {
                startIndex = match.Index + POSITIONCAST_ID.Length;
                endIndex = (i < matches.Count - 1) ? matches[i + 1].Index : rawSpeaker.Length;
                string castPos = rawSpeaker.Substring(startIndex, endIndex - startIndex);

                string[] axis = castPos.Split(AXISDELIMITER, System.StringSplitOptions.RemoveEmptyEntries);

                float.TryParse(axis[0], out castPosition.x);

                if(axis.Length > 1)
                    float.TryParse(axis[1], out castPosition.y);
            }
            else if(match.Value == EXPRESSIONCAST_ID)
            {
                startIndex = match.Index + EXPRESSIONCAST_ID.Length;
                endIndex = (i < matches.Count - 1) ? matches[i + 1].Index : rawSpeaker.Length;
                string castExp = rawSpeaker.Substring(startIndex, endIndex - (startIndex + 1));

                CastExpressions = castExp.Split(EXPERSSIONLAYER_JOINER)
                    .Select(x =>
                    {
                        var parts = x.Trim().Split(EXPERSSIONLAYER_DELIMITER);
                        return (int.Parse(parts[0]), parts[1]);
                    }).ToList();
            }
        }
    }
}
