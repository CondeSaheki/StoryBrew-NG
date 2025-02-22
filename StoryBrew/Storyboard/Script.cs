using StoryBrew.Storyboard.Core;
using StoryBrew.Storyboard.Common;
using StoryBrew.Storyboard.Common.Beatmap;
using StoryBrew.Runtime;

namespace StoryBrew.Storyboard;

public abstract class Script : IDisposable
{
    protected string ProjectPath { get; private set; } = string.Empty;
    protected string MapsetPath { get; private set; } = string.Empty;
    protected Layer Layer { get; private set; } = default;

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
    /// Called when the instance is initialized, use this like a constructor.
    /// Elements registered here are ignored.
    /// </summary>
    public virtual void OnInitialize() { }

    /// <summary>
    /// Registers a storyboard element.
    /// </summary>
    /// <typeparam name="T">The type of the storyboard element to register.</typeparam>
    /// <param name="element">The storyboard element to register.</param>
    public void Register<T>(T element) where T : IElement => collector?.Invoke(element);

    /// <summary>
    /// Registers a storyboard element and outputs the instance.
    /// </summary>
    /// <typeparam name="T">The type of the storyboard element to register.</typeparam>
    /// <param name="element">The storyboard element to register.</param>
    /// <param name="output">The output parameter that holds the registered storyboard element instance.</param>
    public void Register<T>(T element, out T output) where T : IElement
    {
        collector?.Invoke(element);
        output = element;
    }

    /// <summary>
    /// Logs a message.
    /// </summary>
    /// <param name="content">The content to log.</param>
    public void Log(string content) => Runtime.LogSystem.Log.Message($"{GetType().FullName}: {content}");

    private Action<IElement>? collector;

    internal List<IElement> Collect(Beatmap? beatmap = null)
    {
        List<IElement> elements = [];
        collector = elements.Add;
        if (beatmap is not null) Generate(beatmap);
        else Generate();
        collector = null;
        return elements;
    }

    internal void Initialize(Layer layer, ProjectData project)
    {
        ProjectPath = project.DirectoryPath;
        MapsetPath = project.MapsetDirectoryPath;
        Layer = layer;
    }

    public void Dispose() { }
}
