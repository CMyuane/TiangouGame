using System.Collections;
using System.IO;
using UnityEngine;

public class VoiceManager : MonoBehaviour
{
    public static VoiceManager instance { get; private set; }

    [SerializeField] private GameObject recordButton;
    [SerializeField] private GameObject playButton;
    [SerializeField] private AudioSource bgmSource; // ← 拖入 BGM 音源

    private AudioSource voiceSource; // ← 用於播放錄音
    private AudioClip recordedClip;
    private string filePath;
    public bool isRecording = false;
    public static bool isBlockingDialogue { get; private set; } = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        recordButton.SetActive(false);
        playButton.SetActive(false);

        // 建立錄音資料夾
        filePath = Path.Combine(Application.persistentDataPath, "recordings");
        Directory.CreateDirectory(filePath);

        // 初始化語音專用 AudioSource
        voiceSource = gameObject.AddComponent<AudioSource>();
        voiceSource.loop = false;
    }

    public void ShowRecordButton()
    {
        isBlockingDialogue = true;
        recordButton.SetActive(true);
    }

    public void ShowPlayButton()
    {
        isBlockingDialogue = true;
        playButton.SetActive(true);
    }

    public void HideRecordButton()
    {
        isBlockingDialogue = false;
        recordButton.SetActive(false);
    }

    public void HidePlayButton()
    {
        isBlockingDialogue = false;
        playButton.SetActive(false);
    }

    public void ToggleRecording()
    {
        if (!isRecording)
            StartRecording();
        else
        {
            StopRecording();
            HideRecordButton();
        }
    }

    private void StartRecording()
    {
        recordedClip = Microphone.Start(null, false, 60, 44100);
        isRecording = true;
        Debug.Log("Recording started...");
    }

    private void StopRecording()
    {
        if (!isRecording) return;

        int length = Microphone.GetPosition(null);
        Microphone.End(null);

        if (length > 0)
        {
            float[] samples = new float[length * recordedClip.channels];
            recordedClip.GetData(samples, 0);

            AudioClip trimmedClip = AudioClip.Create("Recorded", length, recordedClip.channels, recordedClip.frequency, false);
            trimmedClip.SetData(samples, 0);

            string fullPath = Path.Combine(filePath, "voice.wav");
            WavUtility.SaveWav(fullPath, trimmedClip);
            Debug.Log("Recording saved to: " + fullPath);
        }

        isRecording = false;
    }

    public void PlayRecordedAudio()
    {
        string fullPath = Path.Combine(filePath, "voice.wav");

        if (File.Exists(fullPath))
        {
            AudioClip clip = WavUtility.LoadWav(fullPath);

            if (bgmSource != null && bgmSource.isPlaying)
                bgmSource.Pause();

            voiceSource.clip = clip;
            voiceSource.loop = false;
            voiceSource.Play();

            playButton.SetActive(false);

            Debug.Log("Playing recorded audio.");

            StartCoroutine(ResumeBGM_AfterVoice(clip.length));
        }
        else
        {
            Debug.LogWarning("No recorded file found!");
        }
    }

    private IEnumerator ResumeBGM_AfterVoice(float delay)
    {
        yield return new WaitForSeconds(delay + 0.1f);

        if (bgmSource != null)
            bgmSource.UnPause();

        isBlockingDialogue = false; // 加上這行讓對話可以繼續
    }
}
