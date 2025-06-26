using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  // �� ���s�k�d�v�C����?�g�p�� LayoutGroup

public class DialogueOptionPanel : MonoBehaviour
{
    [SerializeField] private GameObject optionPrefab;
    [SerializeField] private Transform optionContainer;

    public void ShowOptions(List<(string text, System.Action action)> options)
    {
        gameObject.SetActive(true);

        foreach (Transform child in optionContainer)
            Destroy(child.gameObject);

        float startY = 0f;          // ���n Y �ʒu
        float offsetY = 100f;      // ?�I���V�ԓI�ԋ��i���I��\�����r�j

        for (int i = 0; i < options.Count; i++)
        {
            var optionObj = Instantiate(optionPrefab, optionContainer);
            optionObj.GetComponent<DialogueOption>().Setup(options[i].text, options[i].action);

            // �T��?�����I�I���ʒu
            RectTransform rectTransform = optionObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, startY + i * offsetY);
        }
    }


    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
