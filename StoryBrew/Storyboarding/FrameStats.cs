using System.Collections.Generic;

namespace StoryBrew.Storyboarding
{
    public class FrameStats
    {
        public string LastTexture = string.Empty;
        public bool LastBlendingMode;
        public HashSet<string> LoadedPaths = new HashSet<string>();

        public int SpriteCount;
        public int Batches;

        public int CommandCount;
        public int EffectiveCommandCount;
        public bool IncompatibleCommands;
        public bool OverlappedCommands;

        public float ScreenFill;
        public ulong GpuPixelsFrame;
        public double GpuMemoryFrameMb => GpuPixelsFrame / 1024.0 / 1024.0 * 4.0;
    }
}
