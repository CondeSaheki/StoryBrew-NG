using StoryBrew.Storyboarding.CommandValues;
using StoryBrew.Storyboarding.Display;
using StoryBrew.Project.Files;

namespace StoryBrew.Storyboarding;

public class OsbWriterFactory
{
    public static OsbSpriteWriter CreateWriter(Sprite osbSprite, AnimatedValue<CommandPosition> moveTimeline,
                                                                    AnimatedValue<CommandDecimal> moveXTimeline,
                                                                    AnimatedValue<CommandDecimal> moveYTimeline,
                                                                    AnimatedValue<CommandDecimal> scaleTimeline,
                                                                    AnimatedValue<CommandScale> scaleVecTimeline,
                                                                    AnimatedValue<CommandDecimal> rotateTimeline,
                                                                    AnimatedValue<CommandDecimal> fadeTimeline,
                                                                    AnimatedValue<CommandColor> colorTimeline,
                                                                    TextWriter writer, ExportSettings exportSettings, Layer layer)
    {
        if (osbSprite is Animation osbAnimation)
        {
            return new OsbAnimationWriter(osbAnimation, moveTimeline,
                                                        moveXTimeline,
                                                        moveYTimeline,
                                                        scaleTimeline,
                                                        scaleVecTimeline,
                                                        rotateTimeline,
                                                        fadeTimeline,
                                                        colorTimeline,
                                                        writer, exportSettings, layer);
        }
        else
        {
            return new OsbSpriteWriter(osbSprite, moveTimeline,
                                                  moveXTimeline,
                                                  moveYTimeline,
                                                  scaleTimeline,
                                                  scaleVecTimeline,
                                                  rotateTimeline,
                                                  fadeTimeline,
                                                  colorTimeline,
                                                  writer, exportSettings, layer);
        }
    }
}
