using System;
using System.Collections.Generic;
using System.IO;

namespace StoryBrew.Mapset
{
    public class MapsetManager
    {
        private readonly string Path;
        public List<EditorBeatmap> Beatmaps = [];
        
        public MapsetManager(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new Exception("Mapset path cannot be empty");

            Path = path;
            loadBeatmaps();
        }

        private void loadBeatmaps()
        {
            if (!Directory.Exists(Path)) return;

            foreach (var beatmapPath in Directory.GetFiles(Path, "*.osu", SearchOption.TopDirectoryOnly))
            {
                Beatmaps.Add(EditorBeatmap.Load(beatmapPath));
            }
        }
    }
}
