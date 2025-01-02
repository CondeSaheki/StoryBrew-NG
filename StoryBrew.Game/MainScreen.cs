using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.StoryboardsNG;
using osuTK.Graphics;

namespace StoryBrew.Game
{
    public partial class MainScreen : Screen
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            const string osbpath = "/home/saheki/osu-folder/Songs/Various Artists - Anime 2024 Mix/Various Artists - Anime 2024 Mix (Net0)-----------------------------------VIDEO.osb";
            const string mapsetpath = "/home/saheki/osu-folder/Songs/Various Artists - Anime 2024 Mix/";

            if (!File.Exists(osbpath)) throw new FileNotFoundException($"The file {osbpath} does not exist.");
            if (!Directory.Exists(mapsetpath)) throw new DirectoryNotFoundException($"The directory {mapsetpath} does not exist.");


            var decoder = new LegacyStoryboardDecoderRE();
            Storyboard storyboard = new();
            using var resStream = File.OpenRead(osbpath);
            {
                using (var stream = new LineBufferedReader(resStream))
                {
                    storyboard = decoder.Decode(stream);
                }
            }

            InternalChildren =
            [
                storyboard.CreateDrawable(mapsetpath)
            ];
        }
    }
}
