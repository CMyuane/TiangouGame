using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  // ← 這行很重要，因為?使用了 LayoutGroup

public class DialogueOptionPanel : MonoBehaviour
{
    [SerializeField] private GameObject optionPrefab;
    [SerializeField] private Transform optionContainer;

    public void ShowOptions(List<(string text, System.Action action)> options)
    {
        gameObject.SetActive(true);

        foreach (Transform child in optionContainer)
            Destroy(child.gameObject);

        float startY = 0f;          // 初始 Y 位置
        float offsetY = 100f;      // ?個選項之間的間距（負的代表往下排）

        for (int i = 0; i < options.Count; i++)
        {
            var optionObj = Instantiate(optionPrefab, optionContainer);
            optionObj.GetComponent<DialogueOption>().Setup(options[i].text, options[i].action);

            // 控制?個生成的選項位置
            RectTransform rectTransform = optionObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, startY + i * offsetY);
        }
    }


    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
