using UnityEngine;
using System.IO;
using System;

public static class WavUtility
{
    public static void SaveWav(string filePath, AudioClip clip)
    {
        if (!filePath.ToLower().EndsWith(".wav"))
            filePath += ".wav";

        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        File.WriteAllBytes(filePath, ConvertAudioClipToWav(clip));
    }

    public static AudioClip LoadWav(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("WAV file not found at: " + filePath);
            return null;
        }

        byte[] fileBytes = File.ReadAllBytes(filePath);
        return ConvertWavToAudioClip(fileBytes);
    }

    private static byte[] ConvertAudioClipToWav(AudioClip clip)
    {
        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        byte[] wavBytes = ConvertSamplesToWavBytes(samples, clip.channels, clip.frequency);
        return wavBytes;
    }

    private static byte[] ConvertSamplesToWavBytes(float[] samples, int channels, int sampleRate)
    {
        MemoryStream stream = new MemoryStream();

        // WAV header
        stream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
        stream.Write(BitConverter.GetBytes(0), 0, 4); // Placeholder for file size
        stream.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"), 0, 4);
        stream.Write(System.Text.Encoding.UTF8.GetBytes("fmt "), 0, 4);
        stream.Write(BitConverter.GetBytes(16), 0, 4); // PCM chunk size
        stream.Write(BitConverter.GetBytes((ushort)1), 0, 2); // Audio format: PCM
        stream.Write(BitConverter.GetBytes((ushort)channels), 0, 2);
        stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);
        stream.Write(BitConverter.GetBytes(sampleRate * channels * 2), 0, 4);
        stream.Write(BitConverter.GetBytes((ushort)(channels * 2)), 0, 2);
        stream.Write(BitConverter.GetBytes((ushort)16), 0, 2); // Bits per sample

        // data chunk
        stream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
        stream.Write(BitConverter.GetBytes(samples.Length * 2), 0, 4);

        // samples (convert float to 16-bit PCM)
        foreach (var sample in samples)
        {
            short intSample = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
            stream.Write(BitConverter.GetBytes(intSample), 0, 2);
        }

        // Finalize size
        stream.Seek(4, SeekOrigin.Begin);
        stream.Write(BitConverter.GetBytes((int)(stream.Length - 8)), 0, 4);

        return stream.ToArray();
    }

    private static AudioClip ConvertWavToAudioClip(byte[] wavBytes)
    {
        int headerOffset = 44; // WAV header size
        int sampleCount = (wavBytes.Length - headerOffset) / 2;
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            short sample = BitConverter.ToInt16(wavBytes, headerOffset + i * 2);
            samples[i] = sample / 32768f;
        }

        int channels = BitConverter.ToInt16(wavBytes, 22);
        int sampleRate = BitConverter.ToInt32(wavBytes, 24);

        AudioClip clip = AudioClip.Create("LoadedWav", sampleCount, channels, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
