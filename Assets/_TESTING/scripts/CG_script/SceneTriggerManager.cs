using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DIALOGUE;

#region 資料結構
[System.Serializable]
public class SceneImageTrigger
{
    public string keyword;   // 對話出現的關鍵字
    public Texture texture;  // 對應背景或 CG 圖
}
#endregion

public class SceneTriggerManager : MonoBehaviour
{
    /* ───────────── Inspector 設定 ───────────── */
    [Header("背景 RawImage")]
    public RawImage backgroundImage;

    [Header("CG RawImage")]
    public RawImage cgImage;

    [Header("背景觸發設定")]
    public List<SceneImageTrigger> backgroundTriggers = new();

    [Header("CG 觸發設定")]
    public List<SceneImageTrigger> cgTriggers = new();

    [Header("CG 關閉觸發詞（出現時隱藏 CG）")]
    public List<string> cgHideKeywords = new();

    [Header("背景關閉觸發詞（出現時隱藏背景）")]
    public List<string> backgroundHideKeywords = new();

    /* ───────────── 私有成員 ───────────── */
    private readonly Dictionary<string, Texture> backgroundMap = new();
    private readonly Dictionary<string, Texture> cgMap = new();
    private bool isTransitioning;

    /* ───────────── Mono 生命周期 ───────────── */
    private void Awake()
    {
        // 保險：啟動時清貼圖、關物件，避免預設殘留
        if (backgroundImage != null)
        {
            backgroundImage.texture = null;
            backgroundImage.gameObject.SetActive(false);
        }
        if (cgImage != null)
        {
            cgImage.texture = null;
            cgImage.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        // 映射表
        foreach (var t in backgroundTriggers)
            if (!backgroundMap.ContainsKey(t.keyword) && t.texture != null)
                backgroundMap[t.keyword] = t.texture;

        foreach (var t in cgTriggers)
            if (!cgMap.ContainsKey(t.keyword) && t.texture != null)
                cgMap[t.keyword] = t.texture;

        DialogueSystem.instance.onUserPrompt_Next += OnUserPrompt;
    }

    private void OnDestroy()
    {
        if (DialogueSystem.instance != null)
            DialogueSystem.instance.onUserPrompt_Next -= OnUserPrompt;
    }

    /* ───────────── 核心回調 ───────────── */
    private void OnUserPrompt()
    {
        string text = DialogueSystem.instance.dialogueContainer.dialogueText.text.Trim();

        // 背景
        foreach (var kv in backgroundMap)
        {
            if (text.Contains(kv.Key))
            {
                StartCoroutine(SwitchBackground(kv.Value));
                Debug.Log($"背景觸發成功，關鍵字: {kv.Key}");
                goto CG_CHECK;              // 命中後跳出 loop
            }
        }
        Debug.LogWarning($"背景觸發失敗：未找到任何關鍵字。台詞：{text}");

    CG_CHECK:
        // CG
        foreach (var kv in cgMap)
        {
            if (text.Contains(kv.Key))
            {
                StartCoroutine(ShowCG(kv.Value));
                Debug.Log($"CG 觸發成功，關鍵字: {kv.Key}");
                break;
            }
        }

        // 關閉指令
        foreach (string k in backgroundHideKeywords)
            if (text.Contains(k)) { HideBackground(); break; }

        foreach (string k in cgHideKeywords)
            if (text.Contains(k)) { HideCG(); break; }
    }

    /* ───────────── 具體行為 ───────────── */
    private IEnumerator SwitchBackground(Texture newTex)
    {
        EnsureVisible(backgroundImage);

        // 若貼圖不同才需切換與淡轉
        if (backgroundImage.texture != newTex)
        {
            if (isTransitioning) yield break;
            isTransitioning = true;

            yield return FadeOut(backgroundImage);
            backgroundImage.texture = newTex;
            yield return FadeIn(backgroundImage);

            isTransitioning = false;
        }
        else if (!backgroundImage.gameObject.activeSelf)
        {
            // 貼圖相同但處於關閉狀態 → 直接顯示
            backgroundImage.gameObject.SetActive(true);
            yield return FadeIn(backgroundImage, 0.3f);
        }
    }

    private IEnumerator ShowCG(Texture newTex)
    {
        EnsureVisible(cgImage);

        if (cgImage.texture != newTex)
            cgImage.texture = newTex;                // 換圖（或首次設定）

        yield return FadeIn(cgImage);                // 無論如何都淡入顯示
    }

    public void HideBackground() => StartCoroutine(FadeOutAndDisable(backgroundImage));
    public void HideCG() => StartCoroutine(FadeOutAndDisable(cgImage));

    /* ───────────── 輔助 ───────────── */
    /// <summary>確保自身與所有父鏈皆啟用、CanvasGroup α=1、排序足夠高</summary>
    private static void EnsureVisible(RawImage img)
    {
        if (img == null) return;

        // 1. 打開父鏈
        foreach (Transform t in img.transform.GetComponentsInParent<Transform>(true))
            if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);

        // 2. 解除 CanvasGroup 隱形
        foreach (var g in img.transform.GetComponentsInParent<CanvasGroup>(true))
            if (g.alpha == 0) g.alpha = 1f;

        // 3. 最上層排序
        var canvas = img.canvas;
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = 1000;      // 預設足夠高，不怕被遮
        }
    }

    private static IEnumerator FadeOut(RawImage img, float dur = 0.5f)
    {
        Color c = img.color;
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(1f, 0f, t / dur);
            img.color = c;
            yield return null;
        }
        c.a = 0f;
        img.color = c;
    }

    private static IEnumerator FadeIn(RawImage img, float dur = 0.5f)
    {
        Color c = img.color;
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(0f, 1f, t / dur);
            img.color = c;
            yield return null;
        }
        c.a = 1f;
        img.color = c;
    }

    private static IEnumerator FadeOutAndDisable(RawImage img, float dur = 0.5f)
    {
        if (img == null || !img.gameObject.activeSelf) yield break;

        yield return FadeOut(img, dur);
        img.texture = null;
        img.gameObject.SetActive(false);
    }
}
