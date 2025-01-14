using StoryBrew.Mapset;
using StoryBrew.Scripting;
using StoryBrew.Storyboarding;

namespace Storybrew.Scripts;

public class HitObjectHighlight : Script
{
    // [Group("Timing")]
    [Configurable] public int StartTime = 0;
    [Configurable] public int EndTime = 0;
    [Configurable] public int BeatDivisor = 8;

    // [Group("Sprite")]
    [Configurable] public string SpritePath = "sb/glow.png";
    [Configurable] public double SpriteScale = 1;
    [Configurable] public int FadeDuration = 200;

    public override void Generate() { }

    public override void Generate(Beatmap beatmap)
    {
        foreach (var hitobject in beatmap.HitObjects)
        {
            if ((StartTime != 0 || EndTime != 0) &&
                (hitobject.StartTime < StartTime - 5 || EndTime - 5 <= hitobject.StartTime))
                continue;

            var stackOffset = hitobject.StackOffset;

            Register(new OsbSprite(SpritePath, OsbOrigin.Centre, hitobject.Position + stackOffset), out var hSprite);

            hSprite.Scale(OsbEasing.In, hitobject.StartTime, hitobject.EndTime + FadeDuration, SpriteScale, SpriteScale * 0.2);
            hSprite.Fade(OsbEasing.In, hitobject.StartTime, hitobject.EndTime + FadeDuration, 1, 0);
            hSprite.Additive(hitobject.StartTime, hitobject.EndTime + FadeDuration);
            hSprite.Color(hitobject.StartTime, hitobject.Color);

            if (hitobject is OsuSlider)
            {
                var timestep = (beatmap?.GetTimingPointAt((int)hitobject.StartTime)?.BeatDuration ?? throw new Exception())  / BeatDivisor;
                var startTime = hitobject.StartTime;
                while (true)
                {
                    var endTime = startTime + timestep;

                    var complete = hitobject.EndTime - endTime < 5;
                    if (complete) endTime = hitobject.EndTime;

                    var startPosition = hSprite.PositionAt(startTime);
                    hSprite.Move(startTime, endTime, startPosition, hitobject.PositionAtTime(endTime) + stackOffset);

                    if (complete) break;
                    startTime += timestep;
                }
            }
        }
    }
}
