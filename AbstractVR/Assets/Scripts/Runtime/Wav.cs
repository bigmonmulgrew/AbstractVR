using System.IO;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Simple boilerplate utility for converting a unity audio clip to a wav file and saving it to disc.
    /// </summary>
    public static class Wav
    {
        public static void Save(string path, AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogWarning("Cannot save WAV: AudioClip is null");
                return;
            }

            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            using FileStream fileStream = new(path, FileMode.Create);
            using BinaryWriter writer = new(fileStream);

            int sampleCount = samples.Length;
            int frequency = clip.frequency;
            int channels = clip.channels;
            int bitDepth = 16;

            int byteRate = frequency * channels * bitDepth / 8;
            int blockAlign = channels * bitDepth / 8;
            int dataSize = sampleCount * bitDepth / 8;

            // RIFF header
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataSize);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

            // fmt chunk
            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1); // PCM
            writer.Write((short)channels);
            writer.Write(frequency);
            writer.Write(byteRate);
            writer.Write((short)blockAlign);
            writer.Write((short)bitDepth);

            // data chunk
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(dataSize);

            foreach (float sample in samples)
            {
                short intSample = (short)Mathf.Clamp(sample * short.MaxValue, short.MinValue, short.MaxValue);
                writer.Write(intSample);
            }
        }
    }

}
