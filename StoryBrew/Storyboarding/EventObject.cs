﻿namespace StoryBrew.Storyboarding
{
    public interface EventObject
    {
        double EventTime { get; }
        void TriggerEvent(Project project, double currentTime);
    }
}
