using StoryBrew.Storyboarding.Commands;
using StoryBrew.Storyboarding.CommandValues;
using StoryBrew.Storyboarding.Display;
using StoryBrew.Project.Files;

namespace StoryBrew.Storyboarding;

public class OsbAnimationWriter : OsbSpriteWriter
{
    private readonly Animation osbAnimation;
    public OsbAnimationWriter(Animation osbAnimation, AnimatedValue<CommandPosition> moveTimeline,
                                                         AnimatedValue<CommandDecimal> moveXTimeline,
                                                         AnimatedValue<CommandDecimal> moveYTimeline,
                                                         AnimatedValue<CommandDecimal> scaleTimeline,
                                                         AnimatedValue<CommandScale> scaleVecTimeline,
                                                         AnimatedValue<CommandDecimal> rotateTimeline,
                                                         AnimatedValue<CommandDecimal> fadeTimeline,
                                                         AnimatedValue<CommandColor> colorTimeline,
                                                         TextWriter writer, ExportSettings exportSettings, Layer layer)
                                    : base(osbAnimation, moveTimeline,
                                                         moveXTimeline,
                                                         moveYTimeline,
                                                         scaleTimeline,
                                                         scaleVecTimeline,
                                                         rotateTimeline,
                                                         fadeTimeline,
                                                         colorTimeline,
                                                         writer, exportSettings, layer)
    {
        this.osbAnimation = osbAnimation;
    }

    protected override Sprite CreateSprite(List<IFragmentableCommand> segment)
    {
        if (osbAnimation.LoopType == LoopType.LoopOnce && segment.Min(c => c.StartTime) >= osbAnimation.AnimationEndTime)
        {
            //this shouldn't loop again so we need a sprite instead
            var sprite = new Sprite()
            {
                InitialPosition = osbAnimation.InitialPosition,
                Origin = osbAnimation.Origin,
                Path = getLastFramePath(),
            };

            foreach (var command in segment)
                sprite.AddCommand(command);

            return sprite;
        }
        else
        {
            var animation = new Animation()
            {
                Path = osbAnimation.Path,
                InitialPosition = osbAnimation.InitialPosition,
                Origin = osbAnimation.Origin,
                FrameCount = osbAnimation.FrameCount,
                FrameDelay = osbAnimation.FrameDelay,
                LoopType = osbAnimation.LoopType,
            };

            foreach (var command in segment)
                animation.AddCommand(command);

            return animation;
        }
    }

    protected override void WriteHeader(Sprite sprite, StoryboardTransform transform)
    {
        if (sprite is Animation animation)
        {
            var frameDelay = animation.FrameDelay;

            TextWriter.Write($"Animation");
            WriteHeaderCommon(sprite, transform);
            TextWriter.WriteLine($",{animation.FrameCount},{frameDelay.ToString(ExportSettings.NumberFormat)},{animation.LoopType}");
        }
        else base.WriteHeader(sprite, transform);
    }

    protected override HashSet<int> GetFragmentationTimes(IEnumerable<IFragmentableCommand> fragmentableCommands)
    {
        var fragmentationTimes = base.GetFragmentationTimes(fragmentableCommands);

        var tMax = fragmentationTimes.Max();
        var nonFragmentableTimes = new HashSet<int>();

        for (double d = osbAnimation.StartTime; d < osbAnimation.AnimationEndTime; d += osbAnimation.LoopDuration)
        {
            var range = Enumerable.Range((int)d + 1, (int)(osbAnimation.LoopDuration - 1));
            nonFragmentableTimes.UnionWith(range);
        }

        fragmentationTimes.RemoveWhere(t => nonFragmentableTimes.Contains(t) && t < tMax);

        return fragmentationTimes;
    }

    private string getLastFramePath()
    {
        var directory = Path.GetDirectoryName(osbAnimation.Path) ?? throw new InvalidOperationException();
        var file = string.Concat(Path.GetFileNameWithoutExtension(osbAnimation.Path), osbAnimation.FrameCount - 1, Path.GetExtension(osbAnimation.Path));
        return Path.Combine(directory, file);
    }
}
