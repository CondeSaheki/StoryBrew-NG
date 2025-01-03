﻿using StoryBrew.Common.Storyboarding.CommandValues;
using StoryBrew.Common.Storyboarding.Display;

namespace StoryBrew.Common.Storyboarding;

public class OsbWriterFactory
{
    public static OsbSpriteWriter CreateWriter(OsbSprite osbSprite, AnimatedValue<CommandPosition> moveTimeline,
                                                                    AnimatedValue<CommandDecimal> moveXTimeline,
                                                                    AnimatedValue<CommandDecimal> moveYTimeline,
                                                                    AnimatedValue<CommandDecimal> scaleTimeline,
                                                                    AnimatedValue<CommandScale> scaleVecTimeline,
                                                                    AnimatedValue<CommandDecimal> rotateTimeline,
                                                                    AnimatedValue<CommandDecimal> fadeTimeline,
                                                                    AnimatedValue<CommandColor> colorTimeline,
                                                                    TextWriter writer, ExportSettings exportSettings, OsbLayer layer)
    {
        if (osbSprite is OsbAnimation osbAnimation)
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
