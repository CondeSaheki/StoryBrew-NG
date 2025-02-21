/*
    Note: NEEDS TESTING.

    Note: "FFT" stands for "fast fourier transform", a technique used to analyze audio signals and extract their frequency components.

    Note: This feature was present in the old StoryBrew and its logic in now contained in this singletom note that now
    the user is responsible to dispose.

    Note: This is obsolete, providing audio analysis tools is not responsability of the storybrew is recomended to use an external library,
    they can provide this and other features more efficiently.
*/

using ManagedBass;

namespace StoryBrew.Storyboard.Utilities.Audio;

[Obsolete("Providing audio analysis tools is not be responsability of the storybrew is recomended to use an external library")]
public class FftStream : IDisposable
{
    private static readonly Dictionary<string, FftStream> fft_audio_streams = [];
    private int stream;
    private readonly float frequency;
    private bool isDisposed;

    [Obsolete("Providing audio analysis tools is not responsability of the storybrew is recomended to use an external library")]
    public double Duration { get; }

    [Obsolete("Providing audio analysis tools is not responsability of the storybrew is recomended to use an external library")]
    public float Frequency => frequency;

    private FftStream(string path)
    {
        stream = Bass.CreateStream(path, 0, 0, BassFlags.Decode | BassFlags.Prescan);
        if (stream == 0) throw new InvalidOperationException($"Failed to create stream for path: {path}");

        Duration = Bass.ChannelBytes2Seconds(stream, Bass.ChannelGetLength(stream));

        if (!Bass.ChannelGetAttribute(stream, ChannelAttribute.Frequency, out frequency))
            throw new InvalidOperationException("Failed to retrieve frequency attribute.");
    }

    [Obsolete("Providing audio analysis tools is not responsability of the storybrew is recomended to use an external library")]
    public static FftStream GetInstance(string path)
    {
        path = Path.GetFullPath(path);
        if (!fft_audio_streams.TryGetValue(path, out var fftStream))
        {
            fftStream = new FftStream(path);
            fft_audio_streams[path] = fftStream;
        }
        return fftStream;
    }

    [Obsolete("Providing audio analysis tools is not responsability of the storybrew is recomended to use an external library")]
    public static double GetAudioDuration(string path) => GetInstance(path).Duration * 1000;

    [Obsolete("Providing audio analysis tools is not responsability of the storybrew is recomended to use an external library")]
    public static float GetFftFrequency(string path) => GetInstance(path).Frequency;

    [Obsolete("Providing audio analysis tools is not responsability of the storybrew is recomended to use an external library")]
    public static float[] GetFft(double time, string path, bool splitChannels = false) => GetInstance(path).calculateFft(time * 0.001, splitChannels);

    private float[] calculateFft(double time, bool splitChannels)
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);

        Bass.ChannelSetPosition(stream, Bass.ChannelSeconds2Bytes(stream, time));

        var size = 1024 * (splitChannels ? 2 : 1);
        var flags = DataFlags.FFT2048 | (splitChannels ? DataFlags.FFTIndividual : 0);

        var data = new float[size];
        if (Bass.ChannelGetData(stream, data, unchecked((int)flags)) == -1)
            throw new InvalidOperationException($"Failed to get FFT data at {time} seconds.");

        return data;
    }

    [Obsolete("Providing audio analysis tools is not responsability of the storybrew is recomended to use an external library")]
    public static void DisposeAll()
    {
        foreach (var fftStream in fft_audio_streams.Values) fftStream.Dispose();
        fft_audio_streams.Clear();
    }

    [Obsolete("Providing audio analysis tools is not responsability of the storybrew is recomended to use an external library")]
    public void Dispose()
    {
        if (isDisposed) return;

        if (stream != 0)
        {
            Bass.StreamFree(stream);
            stream = 0;
        }
        isDisposed = true;
    }
}
