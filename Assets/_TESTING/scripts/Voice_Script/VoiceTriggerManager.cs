using UnityEngine;
using DIALOGUE;

public class VoiceTriggerManager : MonoBehaviour
{
    private bool hasTriggeredRecord = false;
    private bool hasTriggeredPlay = false;

    void Start()
    {
        DialogueSystem.instance.onUserPrompt_Next += OnUserPrompt;
    }

    void OnDestroy()
    {
        if (DialogueSystem.instance != null)
            DialogueSystem.instance.onUserPrompt_Next -= OnUserPrompt;
    }

    void OnUserPrompt()
    {
        string currentText = DialogueSystem.instance.dialogueContainer.dialogueText.text.Trim();
        Debug.Log($"[VoiceTrigger] 玩家點擊繼續，畫面文字為：{currentText}");

        if (!hasTriggeredRecord && currentText.Contains("搞什么！"))
        {
            hasTriggeredRecord = true;
            Debug.Log("觸發錄音 UI");
            VoiceManager.instance?.ShowRecordButton();
        }

        if (!hasTriggeredPlay && currentText.Contains("今天也很充實呢～"))
        {
            hasTriggeredPlay = true;
            Debug.Log("觸發播放 UI");
            VoiceManager.instance?.ShowPlayButton();
        }
    }
}
