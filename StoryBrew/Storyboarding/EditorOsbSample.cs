﻿// using BrewLib.Audio;
using StoryBrew.Common.Storyboarding;
using System;
using System.IO;

namespace StoryBrew.Storyboarding
{
    public class EditorOsbSample : OsbSample, EventObject
    {
        public double EventTime => Time * 0.001;

        public void TriggerEvent(Project project, double currentTime)
        {
            throw new NotImplementedException();
            // if (EventTime + 1 < currentTime)
            //     return;

            // AudioSample sample;
            // var fullPath = Path.Combine(project.MapsetPath, AudioPath);
            // try
            // {
            //     sample = project.AudioContainer.Get(fullPath);
            //     if (sample == null)
            //     {
            //         fullPath = Path.Combine(project.ProjectAssetFolderPath, AudioPath);
            //         sample = project.AudioContainer.Get(fullPath);
            //     }
            // }
            // catch (IOException)
            // {
            //     // Happens when another process is writing to the file, will try again later.
            //     return;
            // }

            // sample.Play((float)Volume * 0.01f);
        }
    }
}
