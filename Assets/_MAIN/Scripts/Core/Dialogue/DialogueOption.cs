using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueOption : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI optionText;
    [SerializeField] private Button button;

    public void Setup(string text, System.Action onClick)
    {
        optionText.text = text;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => {
            Debug.Log($"?ÍyóπëIçÄ: {text}");
            onClick?.Invoke();
        });
    }
}
