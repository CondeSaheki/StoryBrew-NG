using osu.Framework.Platform;
using osu.Framework;
using StoryBrew.Game;

namespace StoryBrew.Desktop
{
    public static class Program
    {
        public static void Main()
        {
            using (GameHost host = Host.GetSuitableDesktopHost(@"StoryBrew"))
            using (osu.Framework.Game game = new StoryBrewGame())
                host.Run(game);
        }
    }
}
