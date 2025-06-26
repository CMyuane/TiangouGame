using System.Collections;
using System.IO;
using UnityEngine;

public class VoiceManager : MonoBehaviour
{
    public static VoiceManager instance { get; private set; }
    [SerializeField] private GameObject recordButton;
    [SerializeField] private GameObject playButton;
    private AudioClip recordedClip;
    private string filePath;
    public bool isRecording = false;
    public static bool isBlockingDialogue { get; private set; } = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject); // 跨場景保存
        recordButton.SetActive(false);
        playButton.SetActive(false);
        filePath = Path.Combine(Application.persistentDataPath, "recordings");
        Directory.CreateDirectory(filePath);
    }
    public void ShowRecordButton()
    {
        isBlockingDialogue = true; // 顯示時就阻擋
        recordButton.SetActive(true);
    }

    public void ShowPlayButton()
    {
        isBlockingDialogue = true; // 顯示時就阻擋
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
            HideRecordButton(); // 關閉 UI 同時解除阻擋
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
            AudioSource source = GetComponent<AudioSource>();
            if (source == null)
                source = gameObject.AddComponent<AudioSource>();

            source.clip = clip;
            source.Play();
            playButton.gameObject.SetActive(false);
            Debug.Log("Playing recorded audio.");
        }
        else
        {
            Debug.LogWarning("No recorded file found!");
        }
    }
}
