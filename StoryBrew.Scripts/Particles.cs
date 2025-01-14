using OpenTK.Mathematics;
using StoryBrew.Mapset;
using StoryBrew.Scripting;
using StoryBrew.Storyboarding;
using SkiaSharp;

namespace Storybrew.Scripts;

public class Particles : Script
{
    // [Group("Timing")]
    [Configurable] public int StartTime;
    [Configurable] public int EndTime;

    // [Group("Sprite")]
    [Configurable] public string Path = "sb/particle.png";
    [Configurable] public OsbOrigin Origin = OsbOrigin.Centre;
    [Configurable] public Vector2 Scale = new Vector2(1, 1);
    // [Description("Rotation of the sprite; does not influences particle motion direction.")]
    [Configurable] public float Rotation = 0;
    [Configurable] public Color4 Color = Color4.White;
    // [Description("Varies the saturation and brightness of the selected Color for each particle.")]
    [Configurable] public float ColorVariance = 0.6f;
    [Configurable] public bool Additive = false;

    // [Group("Spawn")]
    [Configurable] public int ParticleCount = 32;
    [Configurable] public float Lifetime = 1000;
    // [Description("The point around which particles will be created.")]
    [Configurable] public Vector2 SpawnOrigin = new Vector2(420, 0);
    // [Description("The distance around the Spawn Origin point where particles will be created.")]
    [Configurable] public float SpawnSpread = 360;

    // [Group("Motion")]
    // [Description("The angle in degrees at which particles will be moving.\n0 is to the right, positive values rotate counterclockwise.")]
    [Configurable] public float Angle = 110;
    // [Description("The spread in degrees around Angle.")]
    [Configurable] public float AngleSpread = 60;
    // [Description("The speed at which particles move, in osupixels.")]
    [Configurable] public float Speed = 480;
    // [Description("Eases the motion of particles.")]
    [Configurable] public OsbEasing Easing = OsbEasing.None;

    [Configurable] public int? RngSeed = null;
    private Random random = new Random();

    public override void Generate() { }
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
        using var bitmap = SKBitmap.Decode(Path);

        var duration = (double)(EndTime - StartTime);
        var loopCount = Math.Max(1, (int)Math.Floor(duration / Lifetime));

        for (var i = 0; i < ParticleCount; i++)
        {
            var spawnAngle = (float)(random.NextDouble() * (Math.PI * 2));
            var spawnDistance = (float)(SpawnSpread * Math.Sqrt(random.NextDouble()));

            var moveAngle = MathHelper.DegreesToRadians(Angle + randFloatRange(-AngleSpread, AngleSpread) * 0.5f);
            var moveDistance = Speed * Lifetime * 0.001f;

            var spriteRotation = moveAngle + MathHelper.DegreesToRadians(Rotation);

            var startPosition = SpawnOrigin + new Vector2((float)Math.Cos(spawnAngle), (float)Math.Sin(spawnAngle)) * spawnDistance;
            var endPosition = startPosition + new Vector2((float)Math.Cos(moveAngle), (float)Math.Sin(moveAngle)) * moveDistance;

            var loopDuration = duration / loopCount;
            var startTime = StartTime + (i * loopDuration) / ParticleCount;
            var endTime = startTime + loopDuration * loopCount;

            if (!isVisible(bitmap, startPosition, endPosition, (float)spriteRotation, (float)loopDuration))
                continue;

            var color = Color;
            if (ColorVariance > 0)
            {
                ColorVariance = MathHelper.Clamp(ColorVariance, 0, 1);

                var hsba = Color4.ToHsl(color);
                var sMin = Math.Max(0, hsba.Y - ColorVariance * 0.5f);
                var sMax = Math.Min(sMin + ColorVariance, 1);
                var vMin = Math.Max(0, hsba.Z - ColorVariance * 0.5f);
                var vMax = Math.Min(vMin + ColorVariance, 1);

                color = Color4.FromHsl(new Vector4(
                    hsba.X,
                    randFloatRange(sMin, sMax),
                    randFloatRange(vMin, vMax),
                    hsba.W));
            }

            Register(new OsbSprite(Path, Origin), out var particle);
            if (spriteRotation != 0)
                particle.Rotate(startTime, spriteRotation);
            if (color.R != 1 || color.G != 1 || color.B != 1)
                particle.Color(startTime, color);
            if (Scale.X != 1 || Scale.Y != 1)
            {
                if (Scale.X != Scale.Y)
                    particle.ScaleVec(startTime, Scale.X, Scale.Y);
                else particle.Scale(startTime, Scale.X);
            }
            if (Additive)
                particle.Additive(startTime, endTime);

            particle.StartLoopGroup(startTime, loopCount);
            particle.Fade(OsbEasing.Out, 0, loopDuration * 0.2, 0, color.A);
            particle.Fade(OsbEasing.In, loopDuration * 0.8, loopDuration, color.A, 0);
            particle.Move(Easing, 0, loopDuration, startPosition, endPosition);
            particle.EndGroup();
        }
    }

    private bool isVisible(SKBitmap bitmap, Vector2 startPosition, Vector2 endPosition, float rotation, float duration)
    {
        var spriteSize = new Vector2(bitmap.Width * Scale.X, bitmap.Height * Scale.Y);
        var originVector = OsbSprite.GetOriginVector(Origin, spriteSize.X, spriteSize.Y);

        for (var t = 0; t < duration; t += 200)
        {
            var position = Vector2.Lerp(startPosition, endPosition, t / duration);
            if (OsbSprite.InScreenBounds(position, spriteSize, rotation, originVector))
                return true;
        }
        return false;
    }

    private float randFloatRange(float min, float max) {
        return (float)(random.NextDouble() * (min - max) + min);
    }
}
