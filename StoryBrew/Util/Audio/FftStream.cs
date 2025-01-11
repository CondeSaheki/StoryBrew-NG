using ManagedBass;

namespace StoryBrew.Util.Audio;

internal class FftStream : IDisposable
{
    private int stream;
    private ChannelInfo info;

    private readonly float frequency;

    /// <summary>
    /// Samples per second
    /// </summary>
    public float Frequency => frequency;

    public double Duration { get; }

    public FftStream(string path)
    {
        stream = Bass.CreateStream(path, 0, 0, BassFlags.Decode | BassFlags.Prescan);
        Duration = Bass.ChannelBytes2Seconds(stream, Bass.ChannelGetLength(stream));
        info = Bass.ChannelGetInfo(stream);

        Bass.ChannelGetAttribute(stream, ChannelAttribute.Frequency, out frequency);
    }

    public float[] GetFft(double time, bool splitChannels = false)
    {
        var position = Bass.ChannelSeconds2Bytes(stream, time);
        Bass.ChannelSetPosition(stream, position);

        var size = 1024;
        var flags = DataFlags.FFT2048;

        if (splitChannels)
        {
            size *= info.Channels;
            flags |= DataFlags.FFTIndividual;
        }

        var data = new float[size];

        if (Bass.ChannelGetData(stream, data, unchecked((int)flags)) == -1) throw new InvalidOperationException($"Failed to get FFT data at {time} seconds.");

        return data;
    }

    public void Dispose()
    {
        Bass.StreamFree(stream);
        stream = 0;
    }
}
