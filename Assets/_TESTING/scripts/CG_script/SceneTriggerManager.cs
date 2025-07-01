using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DIALOGUE;

[System.Serializable]
public class SceneImageTrigger
{
    public string keyword;      // 出現在對話中的關鍵字
    public Texture texture;     // 對應的背景或 CG 圖片
}
public class SceneTriggerManager : MonoBehaviour
{
    [Header("背景控制節點（RawImage 的父物件）")]
    public GameObject backgroundPanel;

    [Header("CG 控制節點（RawImage 的父物件）")]
    public GameObject cgPanel;

    [Header("背景 RawImage")]
    public RawImage backgroundImage;

    [Header("CG RawImage")]
    public RawImage cgImage;

    [Header("背景觸發設定")]
    public List<SceneImageTrigger> backgroundTriggers = new();

    [Header("CG 觸發設定")]
    public List<SceneImageTrigger> cgTriggers = new();

    [Header("CG 關閉觸發詞（出現這些詞時會隱藏 CG）")]
    public List<string> cgHideKeywords = new();

    [Header("背景 關閉觸發詞（出現這些詞時會隱藏背景）")]
    public List<string> backgroundHideKeywords = new();

    private Dictionary<string, Texture> backgroundMap = new();
    private Dictionary<string, Texture> cgMap = new();

    private bool isTransitioning = false;

    void Start()
    {
        foreach (var trigger in backgroundTriggers)
        {
            if (!backgroundMap.ContainsKey(trigger.keyword) && trigger.texture != null)
                backgroundMap[trigger.keyword] = trigger.texture;
        }

        foreach (var trigger in cgTriggers)
        {
            if (!cgMap.ContainsKey(trigger.keyword) && trigger.texture != null)
                cgMap[trigger.keyword] = trigger.texture;
        }

        DialogueSystem.instance.onUserPrompt_Next += OnUserPrompt;
    }

    void OnDestroy()
    {
        if (DialogueSystem.instance != null)
            DialogueSystem.instance.onUserPrompt_Next -= OnUserPrompt;
    }

    void OnUserPrompt()
    {
        string text = DialogueSystem.instance.dialogueContainer.dialogueText.text.Trim();

        bool bgTriggered = false;
        bool cgTriggered = false;

        foreach (var entry in backgroundMap)
        {
            if (text.Contains(entry.Key))
            {
                StartCoroutine(SwitchBackground(entry.Value));
                bgTriggered = true;
                Debug.Log($"背景觸發成功，關鍵字: {entry.Key}");
                break;
            }
        }
        if (!bgTriggered)
        {
            Debug.LogWarning($"背景觸發失敗：未找到任何關鍵字。台詞內容：{text}");
        }

        foreach (var entry in cgMap)
        {
            if (text.Contains(entry.Key))
            {
                StartCoroutine(ShowCG(entry.Value));
                cgTriggered = true;
                Debug.Log($"CG觸發成功，關鍵字: {entry.Key}");
                break;
            }
        }
        if (!cgTriggered)
        {
            Debug.LogWarning($"CG觸發失敗：未找到任何關鍵字。台詞內容：{text}");
        }

        foreach (string keyword in backgroundHideKeywords)
        {
            if (text.Contains(keyword))
            {
                HideBackground();
                Debug.Log($"背景隱藏觸發：{keyword}");
                break;
            }
        }

        foreach (string keyword in cgHideKeywords)
        {
            if (text.Contains(keyword))
            {
                HideCG();
                Debug.Log($"CG隱藏觸發：{keyword}");
                break;
            }
        }
    }

    IEnumerator SwitchBackground(Texture newTexture)
    {
        if (isTransitioning || backgroundImage.texture == newTexture)
            yield break;

        isTransitioning = true;

        if (backgroundPanel != null)
            backgroundPanel.SetActive(true);
        else
            Debug.LogWarning("背景控制節點 (backgroundPanel) 尚未指定！");

        yield return FadeOut(backgroundImage);

        backgroundImage.texture = newTexture;
        Debug.Log($"背景圖片切換為：{newTexture?.name ?? "null"}");

        yield return FadeIn(backgroundImage);

        isTransitioning = false;
    }

    IEnumerator ShowCG(Texture cgTexture)
    {
        if (cgImage.texture == cgTexture)
            yield break;

        if (cgPanel != null)
            cgPanel.SetActive(true);
        else
            Debug.LogWarning("CG 控制節點 (cgPanel) 尚未指定！");

        cgImage.texture = cgTexture;
        Debug.Log($"CG 圖片切換為：{cgTexture?.name ?? "null"}");

        yield return FadeIn(cgImage);
    }

    public void HideBackground()
    {
        StartCoroutine(FadeOutAndDisable(backgroundImage, backgroundPanel));
    }

    public void HideCG()
    {
        StartCoroutine(FadeOutAndDisable(cgImage, cgPanel));
    }

    IEnumerator FadeOut(RawImage image, float duration = 0.5f)
    {
        Color c = image.color;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(1f, 0f, t / duration);
            image.color = c;
            yield return null;
        }
        c.a = 0f;
        image.color = c;
    }

    IEnumerator FadeIn(RawImage image, float duration = 0.5f)
    {
        Color c = image.color;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(0f, 1f, t / duration);
            image.color = c;
            yield return null;
        }
        c.a = 1f;
        image.color = c;
    }

    IEnumerator FadeOutAndDisable(RawImage image, GameObject panel, float duration = 0.5f)
    {
        yield return FadeOut(image);
        image.texture = null;
        if (panel != null)
            panel.SetActive(false);
        else
            Debug.LogWarning("嘗試關閉圖片但 panel 為 null。");
    }
}
