using StoryBrew.Mapset;
using System;
using System.IO;

namespace StoryBrew.Storyboarding;

public partial class Project : IDisposable
{
    // Mapset

    public bool MapsetPathIsValid { get; private set; }

    private string mapsetPath;
    public string MapsetPath
    {
        get { return mapsetPath; }
        set
        {
            if (mapsetPath == value) return;
            mapsetPath = value;
            MapsetPathIsValid = Directory.Exists(mapsetPath);

            refreshMapset();
        }
    }

    public MapsetManager MapsetManager { get; private set; }

    private EditorBeatmap? mainBeatmap;
    public EditorBeatmap MainBeatmap
    {
        get
        {
            if (mainBeatmap == null)
                SwitchMainBeatmap();

            return mainBeatmap ?? throw new Exception();
        }
        set
        {
            if (mainBeatmap == value) return;
            mainBeatmap = value;
        }
    }

    public void SwitchMainBeatmap()
    {
        var takeNextBeatmap = false;
        foreach (var beatmap in MapsetManager.Beatmaps)
        {
            if (takeNextBeatmap)
            {
                MainBeatmap = beatmap;
                return;
            }
            else if (beatmap == mainBeatmap)
                takeNextBeatmap = true;
        }
        foreach (var beatmap in MapsetManager.Beatmaps)
        {
            MainBeatmap = beatmap;
            return;
        }
        // MainBeatmap = new EditorBeatmap(null);
        throw new Exception();
    }

    public void SelectBeatmap(long id, string name)
    {
        foreach (var beatmap in MapsetManager.Beatmaps)
            if ((id > 0 && beatmap.Id == id) || (name.Length > 0 && beatmap.Name == name))
            {
                MainBeatmap = beatmap;
                break;
            }
    }

    private void refreshMapset()
    {
        var previousBeatmapId = mainBeatmap?.Id ?? -1;
        var previousBeatmapName = mainBeatmap?.Name;

        mainBeatmap = null;
        MapsetManager = new MapsetManager(mapsetPath);

        if (previousBeatmapName != null)
            SelectBeatmap(previousBeatmapId, previousBeatmapName);
    }
}
