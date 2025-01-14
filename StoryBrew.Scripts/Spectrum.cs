using OpenTK.Mathematics;
using SkiaSharp;
using StoryBrew.Animations;
using StoryBrew.Mapset;
using StoryBrew.Scripting;
using StoryBrew.Storyboarding;

namespace Storybrew.Scripts;

/// <summary>
/// An example of a spectrum effect.
/// </summary>
public class Spectrum : Script
{
    // [Group("Timing")]
    [Configurable] public int StartTime = 0;
    [Configurable] public int EndTime = 10000;
    [Configurable] public int BeatDivisor = 16;

    // [Group("Sprite")]
    [Configurable] public string SpritePath = "sb/bar.png";
    [Configurable] public OsbOrigin SpriteOrigin = OsbOrigin.BottomLeft;
    [Configurable] public Vector2 SpriteScale = new Vector2(1, 100);

    // [Group("Bars")]
    [Configurable] public Vector2 Position = new Vector2(0, 400);
    [Configurable] public float Width = 640;
    [Configurable] public int BarCount = 96;
    [Configurable] public int LogScale = 600;
    [Configurable] public OsbEasing FftEasing = OsbEasing.InExpo;
    [Configurable] public float MinimalHeight = 0.05f;

    // [Group("Optimization")]
    [Configurable] public double Tolerance = 0.2;
    [Configurable] public int CommandDecimals = 1;
    [Configurable] public int FrequencyCutOff = 16000;

    [Configurable] public int? RngSeed = null;
    private Random random = new Random();

    public override void Generate(Beatmap beatmap)
    {
        if (RngSeed != null) {
            random = new Random((int)RngSeed);
        }

        if (StartTime == EndTime && beatmap.HitObjects.FirstOrDefault() != null)
        {
            StartTime = (int)beatmap.HitObjects.First().StartTime;
            EndTime = (int)beatmap.HitObjects.Last().EndTime;
        }
        
        if (StartTime <= EndTime) {
            throw new InvalidOperationException(string.Format("EndTime({0}) must be greater than StartTime{1}", EndTime, StartTime));
        }
        using var bitmap = SKBitmap.Decode(SpritePath);

        var heightKeyframes = new KeyframedValue<float>[BarCount];
        for (var i = 0; i < BarCount; i++)
            heightKeyframes[i] = new KeyframedValue<float>((a, b, c) => 0, 0);

        var fftTimeStep = (beatmap.GetTimingPointAt(StartTime)?.BeatDuration ?? throw new Exception()) / BeatDivisor;
        var fftOffset = fftTimeStep * 0.2;
        for (var time = (double)StartTime; time < EndTime; time += fftTimeStep)
        {
            // TODO: fix this
            var fft = GetFft(time + fftOffset, BarCount, null, FftEasing, FrequencyCutOff);
            for (var i = 0; i < BarCount; i++)
            {
                var height = (float)Math.Log10(1 + fft[i] * LogScale) * SpriteScale.Y / bitmap.Height;
                if (height < MinimalHeight) height = MinimalHeight;

                heightKeyframes[i].Add(time, height);
            }
        }

        var barWidth = Width / BarCount;
        for (var i = 0; i < BarCount; i++)
        {
            var keyframes = heightKeyframes[i];
            keyframes.Simplify1dKeyframes(Tolerance, h => h);

            // var bar = layer.CreateSprite(SpritePath, SpriteOrigin, new Vector2(Position.X + i * barWidth, Position.Y));
            Register(new OsbSprite(SpritePath, SpriteOrigin), out var bar);
            bar.CommandSplitThreshold = 300;
            bar.ColorHsb(StartTime, (i * 360.0 / BarCount) + randFloatRange(-10.0f, 10.0f), 0.6 + randFloatRange(0f, 0.4f), 1);
            bar.Additive(StartTime, EndTime);

            var scaleX = SpriteScale.X * barWidth / bitmap.Width;
            scaleX = (float)Math.Floor(scaleX * 10) / 10.0f;

            var hasScale = false;
            keyframes.ForEachPair(
                (start, end) =>
                {
                    hasScale = true;
                    bar.ScaleVec(start.Time, end.Time,
                        scaleX, start.Value,
                        scaleX, end.Value);
                },
                MinimalHeight,
                s => (float)Math.Round(s, CommandDecimals)
            );
            if (!hasScale) bar.ScaleVec(StartTime, scaleX, MinimalHeight);
        }
    }

    private float randFloatRange(float min, float max) {
        return (float)(random.NextDouble() * (min - max) + min);
    }
}
