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

        if (!hasTriggeredRecord && currentText.Contains("我想幫你完成話劇。"))
        {
            hasTriggeredRecord = true;
            Debug.Log("觸發錄音 UI");
            VoiceManager.instance?.ShowRecordButton();
        }

        if (!hasTriggeredPlay && currentText.Contains("想幫我完成話劇。"))
        {
            hasTriggeredPlay = true;
            Debug.Log("觸發播放 UI");
            VoiceManager.instance?.ShowPlayButton();
        }
    }
}
