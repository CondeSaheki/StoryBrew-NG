// using StoryBrew.Scripting;
// using StoryBrew.Storyboarding;

// namespace Storybrew.Scripts;

// public class Background : StoryboardObjectGenerator
// {
//     [Group("Timing")]
//     [Configurable] public int StartTime = 0;
//     [Configurable] public int EndTime = 0;

//     [Group("Sprite")]
//     [Description("Leave empty to automatically use the map's background.")]
//     [Configurable] public string SpritePath = "";
//     [Configurable] public double Opacity = 0.2;

//     public override void Generate()
//     {
//         if (SpritePath == "") SpritePath = Beatmap.BackgroundPath ?? string.Empty;
//         if (StartTime == EndTime) EndTime = (int)(Beatmap.HitObjects.LastOrDefault()?.EndTime ?? AudioDuration);

//         var bitmap = GetMapsetBitmap(SpritePath);
//         var bg = GetLayer("").CreateSprite(SpritePath, OsbOrigin.Centre);
//         bg.Scale(StartTime, 480.0f / bitmap.Height);
//         bg.Fade(StartTime - 500, StartTime, 0, Opacity);
//         bg.Fade(EndTime, EndTime + 500, Opacity, 0);
//     }
// }


using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenTK.Mathematics;
using SkiaSharp;
using StoryBrew.Mapset;
using StoryBrew.Scripting;
using StoryBrew.Storyboarding;

namespace Storybrew.Scripts;

public class Background : Script
{
    // [Group("Timing")]
    [Configurable] public int StartTime = 0;
    [Configurable] public int EndTime = 0;

    // [Group("Sprite")]
    // [Description("Leave empty to automatically use the map's background.")]
    [Configurable] public string SpritePath = string.Empty;
    [Configurable] public double Opacity = 0.2;

    // to use random values we create a Random class
    [Configurable] public int? RngSeed = null;
    private Random random = new Random();

    // empty init as we need a beatmap for this example
    public override void Generate() { }

    public override void Generate(Beatmap beatmap)
    {
        var path = string.IsNullOrEmpty(SpritePath) ? beatmap.BackgroundPath : SpritePath;

        // we add a new sprite to the storyboard so it gets drawn
        Register(new OsbSprite(path, OsbOrigin.Centre), out var bg);

        // and then we can edit it's properties, which will be shown on the storyboard
        using var bitmap = SKBitmap.Decode(path);
        bg.Scale(StartTime, 480.0f / bitmap.Height);
        bg.Fade(StartTime - 500, StartTime, 0, Opacity);
        bg.Fade(EndTime, EndTime + 500, Opacity, 0);

        if (RngSeed != null) {
            random = new Random((int)RngSeed);
            // TODO: fix this
            bg.Rotate(StartTime, randFloatRange(0, MathHelper.DegreesToRadians(360)));
        }
    }

    private float randFloatRange(float min, float max) {
        return (float)(random.NextDouble() * (min - max) + min);
    }
}
