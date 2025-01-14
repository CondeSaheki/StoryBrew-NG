using OpenTK.Mathematics;
using SkiaSharp;
using StoryBrew.Animations;
using StoryBrew.Mapset;
using StoryBrew.Scripting;
using StoryBrew.Storyboarding;

namespace Storybrew.Scripts;

/// <summary>
/// An example of a radial spectrum effect, using movement instead of scaling.
/// </summary>
public class RadialSpectrum : Script
{
    // [Group("Timing")]
    [Configurable] public int StartTime = 0;
    [Configurable] public int EndTime = 10000;
    [Configurable] public int BeatDivisor = 8;

    // [Group("Sprite")]
    [Configurable] public string SpritePath = "sb/bar.png";
    [Configurable] public OsbOrigin SpriteOrigin = OsbOrigin.Centre;
    [Configurable] public Vector2 SpriteScale = Vector2.One;

    // [Group("Bars")]
    [Configurable] public Vector2 Position = new Vector2(320, 240);
    [Configurable] public int BarCount = 20;
    [Configurable] public int Radius = 50;
    [Configurable] public float Scale = 50;
    [Configurable] public int LogScale = 600;
    [Configurable] public OsbEasing FftEasing = OsbEasing.InExpo;

    // [Group("Optimization")]
    [Configurable] public double Tolerance = 2;
    [Configurable] public int CommandDecimals = 0;
    [Configurable] public int FrequencyCutOff = 16000;

    [Configurable] public int? RngSeed = null;
    private Random random = new Random();

    public override void Generate(Beatmap beatmap)
    {
        if (StartTime == EndTime && beatmap.HitObjects.FirstOrDefault() != null)
        {
            StartTime = (int)beatmap.HitObjects.First().StartTime;
            EndTime = (int)beatmap.HitObjects.Last().EndTime;
        }
        
        if (StartTime <= EndTime) {
            throw new InvalidOperationException(string.Format("EndTime({0}) must be greater than StartTime{1}", EndTime, StartTime));
        }
        using var bitmap = SKBitmap.Decode(SpritePath);

        var positionKeyframes = new KeyframedValue<Vector2>[BarCount];

        for (var i = 0; i < BarCount; i++)
            positionKeyframes[i] = new KeyframedValue<Vector2>((a, b, c) => Vector2.Zero, Vector2.Zero);

        var fftTimeStep = (beatmap.GetTimingPointAt(StartTime)?.BeatDuration ?? throw new Exception()) / BeatDivisor;
        var fftOffset = fftTimeStep * 0.2;
        for (var time = (double)StartTime; time < EndTime; time += fftTimeStep)
        {
            // TODO: fix this
            var fft = GetFft(time + fftOffset, BarCount, null, FftEasing, FrequencyCutOff);
            for (var i = 0; i < BarCount; i++)
            {
                var height = Radius + (float)Math.Log10(1 + fft[i] * LogScale) * Scale;

                var angle = i * (Math.PI * 2) / BarCount;
                var offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * height;

                positionKeyframes[i].Add(time, Position + offset);
            }
        }

        var barScale = ((Math.PI * 2 * Radius) / BarCount) / bitmap.Width;
        for (var i = 0; i < BarCount; i++)
        {
            var keyframes = positionKeyframes[i];
            keyframes.Simplify2dKeyframes(Tolerance, h => h);

            var angle = i * (Math.PI * 2) / BarCount;
            var defaultPosition = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Radius;

            Register(new OsbSprite(SpritePath, SpriteOrigin), out var bar);
            bar.CommandSplitThreshold = 300;
            bar.ColorHsb(StartTime, (i * 360.0 / BarCount) + randFloatRange(-10.0f, 10.0f), 0.6 + randFloatRange(0f, 0.4f), 1);
            if (SpriteScale.X == SpriteScale.Y)
                bar.Scale(StartTime, barScale * SpriteScale.X);
            else bar.ScaleVec(StartTime, barScale * SpriteScale.X, barScale * SpriteScale.Y);
            bar.Rotate(StartTime, angle);
            bar.Additive(StartTime, EndTime);

            var hasMove = false;
            keyframes.ForEachPair(
                (start, end) =>
                {
                    hasMove = true;
                    bar.Move(start.Time, end.Time, start.Value, end.Value);
                },
                defaultPosition,
                s => new Vector2((float)Math.Round(s.X, CommandDecimals), (float)Math.Round(s.Y, CommandDecimals))
            );
            if (!hasMove) bar.Move(StartTime, defaultPosition);
        }
    }

    private float randFloatRange(float min, float max) {
        return (float)(random.NextDouble() * (min - max) + min);
    }
}
