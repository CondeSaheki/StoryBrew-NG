using StoryBrew.Common.Mapset;
using StoryBrew.Common.Storyboarding;

namespace StoryBrew;

public abstract class Script
{
    public string ProjectPath { get; private set; } = string.Empty;
    public string MapsetPath { get; private set; } = string.Empty;
    public string AssetPath { get; private set; } = string.Empty; // Maybe remove
    public object Layer { get; private set; } = null!;

    /// <summary>
    /// Generates the storyboard.
    /// This is the main entry point when generating a storyboard.
    /// </summary>
    public virtual void Generate() { }

    /// <summary>
    /// Generates the storyboard.
    /// This is the main entry point when generating a storyboard into the beatmap.
    /// </summary>
    public virtual void Generate(Beatmap beatmap) { }

    /// <summary>
    /// Registers a storyboard object instance.
    /// </summary>
    /// <typeparam name="T">The type of the storyboard object to register.</typeparam>
    /// <param name="instance">The storyboard object instance to register.</param>
    public void Register<T>(T instance) where T : StoryboardObject => collector?.Invoke(instance);

    /// <summary>
    /// Registers a storyboard object instance and outputs the same instance.
    /// </summary>
    /// <typeparam name="T">The type of the storyboard object to register.</typeparam>
    /// <param name="instance">The storyboard object instance to register.</param>
    /// <param name="obj">The output parameter that holds the registered storyboard object instance.</param>
    public void Register<T>(T instance, out T obj) where T : StoryboardObject
    {
        collector?.Invoke(instance);
        obj = instance;
    }

    private Action<StoryboardObject>? collector;

    internal List<StoryboardObject> Collect()
    {
        List<StoryboardObject> osbObjects = [];
        collector = osbObjects.Add;
        Generate();
        collector = null;
        return osbObjects;
    }

    internal List<StoryboardObject> Collect(Beatmap beatmap)
    {
        List<StoryboardObject> osbObjects = [];
        collector = osbObjects.Add;
        Generate(beatmap);
        collector = null;
        return osbObjects;
    }

    internal Dictionary<Beatmap, List<StoryboardObject>> Collect(List<Beatmap> beatmaps)
    {
        Dictionary<Beatmap, List<StoryboardObject>> BeatmapObjects = [];
        foreach (var beatmap in beatmaps) BeatmapObjects[beatmap] = Collect(beatmap);
        collector = null;
        return BeatmapObjects;
    }

    internal void Init(object layer, Project project)
    {
        if (ProjectPath != string.Empty || MapsetPath != string.Empty || AssetPath != string.Empty) throw new Exception("Already initialized.");

        ProjectPath = project.ProjectDirectoryPath;
        MapsetPath = project.MapsetDirectoryPath;
        AssetPath = project.AssetsDirectoryPath;
    }
}
