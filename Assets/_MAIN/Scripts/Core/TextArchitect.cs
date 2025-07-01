using System.Collections;
using UnityEngine;
using TMPro;

//ÎÄ±¾¹¹½¨Æ÷£¬ÊµÏÖÁË¼¸ÖÖÎÄ±¾ÏÔÊ¾
public class TextArchitect
{
    //ÒıÓÃÁ½ÖÖTextMeshPro×é¼ş£¨»ùÓÚui¡¢»ùÓÚÊÀ½ç¿Õ¼ä£©
    private TextMeshProUGUI tmpro_ui;
    private TextMeshPro tmpro_world;

    //Í³Ò»·ÃÎÊ½Ó¿Ú£¬ÓÅÏÈ·µ»Øui×é¼ş
    public TMP_Text tmpro => tmpro_ui != null ? tmpro_ui : tmpro_world;  //¼EétmprouiÊÇ·ñ²»Îª¿Õ£¬²»Îª¿ÕÊ¹ÓÃui£¬Îª¿ÕÊ¹ÓÃworld

    //µ±Ç°ÏÔÊ¾µÄÎÄ±¾ÄÚÈİ
    public string currentText => tmpro.text;
    //Ä¿±EÄ±¾£¨ĞÂÄÚÈİ£©
    public string targetText { get; private set; } = "";
    //Ç°ÖÃÎÄ±¾£¨ÒÑ¾­´æÔÚµÄÄÚÈİ£©
    public string preText { get; private set; } = "";                   //ĞÂÎÄ±¾¿É¸½¼Óµ½ÏÖÓĞÎÄ±¾£¨ÒÑÓĞµÄÈÎºÎÄÚÈİ£©Ö®ÉÏ
    private int preTextLength = 0;                                      //Ç°ÖÃÎÄ±¾³¤¶È£¨ÓÃÓÚµ­ÈE
    //ÍEûÄ¿±EÄ±¾£¨Ç°ÖÃ+Ä¿±ê£©
    public string fullTargetText => preText + targetText;


    //¹¹½¨·½Ê½Ã¶¾Ù£¬Ä¬ÈÏÎª´ò×Ö»E
    public enum BuildMethod { instant, typewriter, fade }
    public BuildMethod buildMethod = BuildMethod.typewriter;            

    //ÎÄ±¾±äÁ¿ ¿ØÖÆÎÄ±¾ÑÕÉ«
    public Color textColor { 
        get { return tmpro.color; } 
        set { tmpro.color = value; }
    }

    //ËÙ¶È±äÁ¿ ¿ØÖÆ¶Ô»°ÏÔÊ¾ËÙ¶È£¨»ù´¡ËÙ¶Èx±¶ÂÊ£©
    public float speed { 
        get { return baseSpeed * speedMultiplier; } 
        set { speedMultiplier = value; }
    }

    private const float baseSpeed = 1;
    private float speedMultiplier = 1;

    //Ã¿¸öÉú³ÉÖÜÆÚµÄ×Ö·ûÊı £¨ÉèÖÃÎÄ×Ö³öÏÖËÙ¶È£©Ä¬ÈÏ¼ÓËÙ¹Ø±Õ
    public int charactersPerCycle { get { return speed <= 2f ? charactersMultiplier : speed <= 2.5f ? charactersMultiplier * 2 : charactersMultiplier * 3; } }
    private int charactersMultiplier = 1;

    public bool hurryUp = false;

    //¹¹ÔE¯Êı
    public TextArchitect(TextMeshProUGUI tmpro_ui)
    {
        this.tmpro_ui = tmpro_ui;
    }

    public TextArchitect(TextMeshPro tmpro_world)
    {
        this.tmpro_world = tmpro_world;                         //¿ÉÒÔ·ÖÅäui»Ed¿Õ¼ä¸øtextarchitect
    }

    //¹¹½¨ÎÄ±¾
    public Coroutine Build(string text)
    {
        preText = "";                       //Èç¹ûÕıÔÚ¹¹½¨£¬±£Ö¤Ç°ÖÃÎÄ±¾Îª¿Õ
        targetText = text;          //Ä¿±EÄ±¾µÈÓÚ´«ÈEÄÈÎºÎÎÄ±¾

        Stop();

        buildProcess = tmpro.StartCoroutine(Building()); //È·±£Ã»ÓĞÕıÔÚÔËĞĞµÄ¹¹½¨¹ı³Ì buildProcess½«´¢´æÕıÔÚÔËĞĞµÄ½ø³Ì
        return buildProcess;        //·µ»Ø¹¹½¨¹ı³Ì
    }

    //¸½¼ÓÎÄ±¾
    public Coroutine Append(string text)
    {
        preText = tmpro.text;
        targetText = text;

        Stop();

        buildProcess = tmpro.StartCoroutine(Building());
        return buildProcess;
    }
    //¼àÊÓÆ÷·½·¨£¬Èç¹û¹¹½¨ÀàÕıÔÚÉú³ÉÔòÍ£Ö¹ËE
    private Coroutine buildProcess = null;      //´¦ÀúêÄ±¾Éú³É
    public bool isBuilding => buildProcess != null;     //Èç¹ûÕıÔÚ¹¹½¨·µ»ØT

    //Í£Ö¹ÕıÔÚÔËĞĞµÄĞ­Í¬³ÌĞE
    public void Stop()
    {
        if (!isBuilding)
            return;

        tmpro.StopCoroutine(buildProcess);
        buildProcess = null;
    }

    //¹¹½¨·½·¨Ã¶¾ÙÆE
    IEnumerator Building()
    {
        Prepare();

        switch(buildMethod)
        {
            case BuildMethod.typewriter:
                yield return Build_Typewriter();
                break;
            case BuildMethod.fade:
                yield return Build_Fade();
                break;
        }

        OnComplete();
    }

    private void OnComplete()
    {
        buildProcess = null;
        hurryUp = false;
    }

    public void ForceComplete()
    {
        switch(buildMethod)
        {
            case BuildMethod.typewriter:
                tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
                break;
            case BuildMethod.fade:
                tmpro.ForceMeshUpdate();
                break;
        }

        Stop();
        OnComplete();
    }

    private void Prepare()
    {
        switch(buildMethod)
        {
            case BuildMethod.instant:
                Prepare_Instant();
                break;
            case BuildMethod.typewriter:
                Prepare_Typewriter();
                break;
            case BuildMethod.fade:
                Prepare_Fade();
                break;

        }
    }

    private void Prepare_Instant()
    {
        tmpro.color = tmpro.color;
        tmpro.text = fullTargetText;
        tmpro.ForceMeshUpdate();
        tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
    }

    private void Prepare_Typewriter()
    {
        tmpro.color = tmpro.color;
        tmpro.maxVisibleCharacters = 0;
        tmpro.text = preText;

        if(preText!="")
        {
            tmpro.ForceMeshUpdate();
            tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
        }

        tmpro.text += targetText;
        tmpro.ForceMeshUpdate();
    }

    private void Prepare_Fade()
    {
        tmpro.text = preText;
        if(preText !="")
        {
            tmpro.ForceMeshUpdate();
            preTextLength = tmpro.textInfo.characterCount;
        }
        else
            preTextLength = 0;

        tmpro.text += targetText;
        tmpro.maxVisibleCharacters = int.MaxValue;
        tmpro.ForceMeshUpdate();

        TMP_TextInfo textInfo = tmpro.textInfo;

        Color colorVisable = new Color(textColor.r, textColor.g, textColor.b, 1);
        Color colorHidden = new Color(textColor.r, textColor.g, textColor.b, 0);

        Color32[] vertexColors = textInfo.meshInfo[textInfo.characterInfo[0].materialReferenceIndex].colors32;
        
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible)
                continue;

            if(i < preTextLength)
            {
                for (int v = 0; v < 4; v++)
                    vertexColors[charInfo.vertexIndex + v] = colorVisable;
            }
            else
            {
                for (int v = 0; v < 4; v++)
                    vertexColors[charInfo.vertexIndex + v] = colorHidden;
            }
        }

        tmpro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    private IEnumerator Build_Typewriter()
    {
        while(tmpro.maxVisibleCharacters<tmpro.textInfo.characterCount)
        {
            tmpro.maxVisibleCharacters += hurryUp ? charactersPerCycle * 5 : charactersPerCycle;

            yield return new WaitForSeconds(0.015f / speed);
        }
    }
    private IEnumerator Build_Fade()
    {
        int mainRange = preTextLength;
        int maxRange = mainRange + 1;

        byte alphaThreshold = 15;

        TMP_TextInfo textInfo = tmpro.textInfo;

        Color32[] vertexColors = textInfo.meshInfo[textInfo.characterInfo[0].materialReferenceIndex].colors32;
        float[] alphas = new float[textInfo.characterCount];

        while(true)
        {
            float fadeSpeed = ((hurryUp ? charactersPerCycle * 5 : charactersPerCycle) * speed) * 4f;

            for (int i = mainRange; i < maxRange; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                if (!charInfo.isVisible)
                    continue;

                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                alphas[i] = Mathf.MoveTowards(alphas[i], 255, fadeSpeed);

                for (int v = 0; v < 4; v++)
                    vertexColors[charInfo.vertexIndex + v].a = (byte)alphas[i];

                if (alphas[i] >= 255)
                    mainRange++;
            }
            tmpro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            bool lastCharacterIsInvisible = !textInfo.characterInfo[maxRange - 1].isVisible;
            if (alphas[maxRange - 1] > alphaThreshold || lastCharacterIsInvisible)
            {
                if (maxRange < textInfo.characterCount)
                    maxRange++;
                else if (alphas[maxRange - 1] >= 255 || lastCharacterIsInvisible)
                    break;
            }

            yield return new WaitForEndOfFrame();
        }
        
    }

}
