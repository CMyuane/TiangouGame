using System.Collections;
using UnityEngine;
using TMPro;

//文本构建器，实现了几种文本显示
public class TextArchitect
{
    //引用两种TextMeshPro组件（基于ui、基于世界空间）
    private TextMeshProUGUI tmpro_ui;
    private TextMeshPro tmpro_world;

    //统一访问接口，优先返回ui组件
    public TMP_Text tmpro => tmpro_ui != null ? tmpro_ui : tmpro_world;  //检查tmproui是否不为空，不为空使用ui，为空使用world

    //当前显示的文本内容
    public string currentText => tmpro.text;
    //目标文本（新内容）
    public string targetText { get; private set; } = "";
    //前置文本（已经存在的内容）
    public string preText { get; private set; } = "";                   //新文本可附加到现有文本（已有的任何内容）之上
    private int preTextLength = 0;                                      //前置文本长度（用于淡入)
    //完整目标文本（前置+目标）
    public string fullTargetText => preText + targetText;


    //构建方式枚举，默认为打字机
    public enum BuildMethod { instant, typewriter, fade }
    public BuildMethod buildMethod = BuildMethod.typewriter;            

    //文本变量 控制文本颜色
    public Color textColor { 
        get { return tmpro.color; } 
        set { tmpro.color = value; }
    }

    //速度变量 控制对话显示速度（基础速度x倍率）
    public float speed { 
        get { return baseSpeed * speedMultiplier; } 
        set { speedMultiplier = value; }
    }

    private const float baseSpeed = 1;
    private float speedMultiplier = 1;

    //每个生成周期的字符数 （设置文字出现速度）默认加速关闭
    public int charactersPerCycle { get { return speed <= 2f ? charactersMultiplier : speed <= 2.5f ? charactersMultiplier * 2 : charactersMultiplier * 3; } }
    private int charactersMultiplier = 1;

    public bool hurryUp = false;

    //构造函数
    public TextArchitect(TextMeshProUGUI tmpro_ui)
    {
        this.tmpro_ui = tmpro_ui;
    }

    public TextArchitect(TextMeshPro tmpro_world)
    {
        this.tmpro_world = tmpro_world;                         //可以分配ui或3d空间给textarchitect
    }

    //构建文本
    public Coroutine Build(string text)
    {
        preText = "";                       //如果正在构建，保证前置文本为空
        targetText = text;          //目标文本等于传入的任何文本

        Stop();

        buildProcess = tmpro.StartCoroutine(Building()); //确保没有正在运行的构建过程 buildProcess将储存正在运行的进程
        return buildProcess;        //返回构建过程
    }

    //附加文本
    public Coroutine Append(string text)
    {
        preText = tmpro.text;
        targetText = text;

        Stop();

        buildProcess = tmpro.StartCoroutine(Building());
        return buildProcess;
    }
    //监视器方法，如果构建类正在生成则停止他
    private Coroutine buildProcess = null;      //处理文本生成
    public bool isBuilding => buildProcess != null;     //如果正在构建返回T

    //停止正在运行的协同程序
    public void Stop()
    {
        if (!isBuilding)
            return;

        tmpro.StopCoroutine(buildProcess);
        buildProcess = null;
    }

    //构建方法枚举器
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
