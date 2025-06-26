using UnityEngine;
using UnityEngine.UI;

public class VoiceButtonController : MonoBehaviour
{
    public Button recordButton;
    public Button playButton;

    private void Start()
    {
        recordButton.onClick.AddListener(() => VoiceManager.instance.ToggleRecording());
        playButton.onClick.AddListener(() => VoiceManager.instance.PlayRecordedAudio());
    }
}
