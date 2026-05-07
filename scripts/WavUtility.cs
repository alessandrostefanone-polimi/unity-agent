using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    public static byte[] FromAudioClip(AudioClip clip, int samplesToUse)
    {
        if (samplesToUse <= 0)
        {
            samplesToUse = clip.samples;
        }

        float[] samples = new float[samplesToUse * clip.channels];
        clip.GetData(samples, 0);

        byte[] pcmData = ConvertFloatTo16BitPCM(samples);

        using (MemoryStream stream = new MemoryStream())
        {
            WriteWavHeader(
                stream,
                pcmData.Length,
                clip.channels,
                clip.frequency
            );

            stream.Write(pcmData, 0, pcmData.Length);
            return stream.ToArray();
        }
    }

    private static byte[] ConvertFloatTo16BitPCM(float[] samples)
    {
        byte[] pcmData = new byte[samples.Length * 2];

        int offset = 0;

        foreach (float sample in samples)
        {
            short value = (short)Mathf.Clamp(
                sample * short.MaxValue,
                short.MinValue,
                short.MaxValue
            );

            byte[] bytes = BitConverter.GetBytes(value);
            pcmData[offset++] = bytes[0];
            pcmData[offset++] = bytes[1];
        }

        return pcmData;
    }

    private static void WriteWavHeader(
        Stream stream,
        int pcmDataLength,
        int channels,
        int sampleRate
    )
    {
        int byteRate = sampleRate * channels * 2;

        using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + pcmDataLength);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write((short)(channels * 2));
            writer.Write((short)16);

            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(pcmDataLength);
        }
    }
}