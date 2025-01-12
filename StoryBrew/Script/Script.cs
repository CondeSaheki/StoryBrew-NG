using StoryBrew.Mapset;
using StoryBrew.Storyboarding;

namespace StoryBrew.Project;

public abstract class Script : IDisposable
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

    internal void Init(object layer, Manager project)
    {
        if (ProjectPath != string.Empty || MapsetPath != string.Empty || AssetPath != string.Empty) throw new Exception("Already initialized.");

        ProjectPath = project.ProjectDirectoryPath;
        MapsetPath = project.MapsetDirectoryPath;
        AssetPath = project.AssetsDirectoryPath;
    }

    public void Dispose() { }
}

/*
    StoryboardObjectGenerator.cs

using SkiaSharp;
using StoryBrew.Animations;
using StoryBrew.Mapset;
using StoryBrew.Storyboarding;
using StoryBrew.Util;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace StoryBrew.Scripting;

public abstract class StoryboardObjectGenerator
{
    [ThreadStatic]
    public static StoryboardObjectGenerator? Current;

    private List<ConfigurableField> configurableFields = [];
    private GeneratorContext? context;

    /// <summary>
    /// Override to true if this script uses multiple threads.
    /// It will prevent other effects from updating in parallel to this one.
    /// </summary>
    public virtual bool Multithreaded => false;

    /// <summary>
    /// Creates or retrieves a layer.
    /// The identifier will be shown in the editor as "Effect name (Identifier)".
    /// Layers will be sorted by the order in which they are first retrieved.
    /// </summary>
    public StoryboardLayer GetLayer(string identifier) => context?.GetLayer(identifier) ?? throw new Exception();

    public Beatmap Beatmap => context?.Beatmap  ?? throw new Exception();
    public Beatmap GetBeatmap(string name)
        => context?.Beatmaps.FirstOrDefault(b => b.Name == name) ?? throw new Exception();

    public string ProjectPath => context?.ProjectPath ?? throw new Exception();
    public string AssetPath => context?.ProjectAssetPath ?? throw new Exception();
    public string MapsetPath => context?.MapsetPath ?? throw new Exception();

    public StoryboardObjectGenerator(GeneratorContext? context = null)
    {
        this.context = context;
        initializeConfigurableFields();
    }

    public void AddDependency(string path)
        => context?.AddDependency(path);

    public void Log(string message)
        => context?.AppendLog(message);

    public void Log(object message)
        => Log(message.ToString() ?? string.Empty);

    public void Assert(bool condition, string? message = null, [CallerLineNumber] int line = -1)
    {
        if (!condition)
            throw new Exception(message != null ? $"Assertion failed line {line}: {message}" : $"Assertion failed line {line}");
    }

    #region File loading

    private readonly Dictionary<string, SKBitmap> bitmaps = [];

    /// <summary>
    /// Returns a SKBitmap from the project's directory.
    /// Do not call Dispose, it will be disposed automatically when the script ends.
    /// </summary>
    public SKBitmap GetProjectBitmap(string path, bool watch = true) => getBitmap(Path.Combine(context?.ProjectPath ?? throw new Exception(), path), null, watch);

    /// <summary>
    /// Returns a SKBitmap from the mapset's directory.
    /// Do not call Dispose, it will be disposed automatically when the script ends.
    /// </summary>
    public SKBitmap GetMapsetBitmap(string path, bool watch = true)
        => getBitmap(Path.Combine(context?.MapsetPath ?? throw new Exception(), path), Path.Combine(context.ProjectAssetPath, path), watch);

    private SKBitmap getBitmap(string path, string? alternatePath, bool watch)
    {
        path = Path.GetFullPath(path);

        if (!bitmaps.TryGetValue(path, out SKBitmap? bitmap))
        {
            if (watch) context?.AddDependency(path);

            if (alternatePath != null && !File.Exists(path))
            {
                alternatePath = Path.GetFullPath(alternatePath);
                if (watch) context?.AddDependency(alternatePath);

                try
                {
                    bitmap = loadSKBitmap(alternatePath);
                    bitmaps.Add(path, bitmap);
                }
                catch (FileNotFoundException e)
                {
                    throw new FileNotFoundException(path, e);
                }
            }
            else
            {
                bitmap = loadSKBitmap(path);
                bitmaps.Add(path, bitmap);
            }
        }
        return bitmap;
    }

    private static SKBitmap loadSKBitmap(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        return SKBitmap.Decode(stream) ?? throw new InvalidOperationException($"Failed to decode bitmap from {filePath}");
    }


    /// <summary>
    /// Opens a project file in read-only mode.
    /// You are responsible for disposing it.
    /// </summary>
    public Stream OpenProjectFile(string path, bool watch = true) => openFile(Path.Combine(context?.ProjectPath ?? throw new Exception(), path), watch);

    /// <summary>
    /// Opens a mapset file in read-only mode.
    /// You are responsible for disposing it.
    /// </summary>
    public Stream OpenMapsetFile(string path, bool watch = true) => openFile(Path.Combine(context?.MapsetPath ?? throw new Exception(), path), watch);

    private Stream openFile(string path, bool watch)
    {
        path = Path.GetFullPath(path);
        if (watch) context?.AddDependency(path);
        return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    #endregion

    #region Random

    [Group("Common")]
    [Description("Changes the result of Random(...) calls.")]
    [Configurable] public int RandomSeed;

    private Random random = new();
    public int Random(int minValue, int maxValue) => random.Next(minValue, maxValue);
    public int Random(int maxValue) => random.Next(maxValue);
    public double Random(double minValue, double maxValue) => minValue + random.NextDouble() * (maxValue - minValue);
    public double Random(double maxValue) => random.NextDouble() * maxValue;
    public float Random(float minValue, float maxValue) => minValue + (float)random.NextDouble() * (maxValue - minValue);
    public float Random(float maxValue) => (float)random.NextDouble() * maxValue;

    #endregion

    #region Audio data

    public double AudioDuration => context?.AudioDuration ?? throw new Exception();

    /// <summary>
    /// Returns the Fast Fourier Transform of the song at a certain time, with the default amount of magnitudes.
    /// Useful to make spectrum effets.
    /// </summary>
    public float[] GetFft(double time, string? path = null, bool splitChannels = false)
    {
        if (path != null) AddDependency(path);
        return context?.GetFft(time, path, splitChannels) ?? throw new Exception();
    }

    /// <summary>
    /// Returns the Fast Fourier Transform of the song at a certain time, with the specified amount of magnitudes.
    /// Useful to make spectrum effets.
    /// </summary>
    public float[] GetFft(double time, int magnitudes, string? path = null, OsbEasing easing = OsbEasing.None, float frequencyCutOff = 0)
    {
        var fft = GetFft(time, path);
        if (magnitudes == fft.Length && easing == OsbEasing.None)
            return fft;

        var usedFftLength = frequencyCutOff > 0 ?
            (int)Math.Floor(frequencyCutOff / ((context?.GetFftFrequency(path) ?? throw new Exception()) / 2.0) * fft.Length) :
            fft.Length;

        var resultFft = new float[magnitudes];
        var baseIndex = 0;
        for (var i = 0; i < magnitudes; i++)
        {
            var progress = EasingFunctions.Ease(easing, (double)i / magnitudes);
            var index = Math.Min(Math.Max(baseIndex + 1, (int)(progress * usedFftLength)), usedFftLength - 1);

            var value = 0f;
            for (var v = baseIndex; v < index; v++)
                value = Math.Max(value, fft[index]);

            resultFft[i] = value;
            baseIndex = index;
        }
        return resultFft;
    }

    #endregion

    #region Subtitles



    private readonly SrtParser srtParser = new();
    private readonly AssParser assParser = new();
    private readonly SbvParser sbvParser = new();

    private readonly HashSet<string> fontDirectories = [];
    private readonly List<FontGenerator> fontGenerators = [];

    private string fontCacheDirectory => Path.Combine(context?.ProjectPath ?? throw new Exception(), ".cache", "font");

    public SubtitleSet LoadSubtitles(string path)
    {
        path = Path.Combine(context?.ProjectPath ?? throw new Exception(), path);
        context.AddDependency(path);

        switch (Path.GetExtension(path))
        {
            case ".srt": return srtParser.Parse(path);
            case ".ssa":
            case ".ass": return assParser.Parse(path);
            case ".sbv": return sbvParser.Parse(path);
        }
        throw new NotSupportedException($"{Path.GetExtension(path)} isn't a supported subtitle format");
    }

    public FontGenerator LoadFont(string directory, FontDescription description, params FontEffect[] effects)
        => LoadFont(directory, false, description, effects);

    public FontGenerator LoadFont(string directory, bool asAsset, FontDescription description, params FontEffect[] effects)
    {
        var assetDirectory = (asAsset ? context?.ProjectAssetPath : context?.MapsetPath) ?? throw new Exception();

        var fontDirectory = Path.GetFullPath(Path.Combine(assetDirectory, directory));
        if (fontDirectories.Contains(fontDirectory))
            throw new InvalidOperationException($"This effect already generated a font inside \"{fontDirectory}\"");
        fontDirectories.Add(fontDirectory);

        var fontGenerator = new FontGenerator(directory, description, effects, context?.ProjectPath ?? throw new Exception(), assetDirectory);
        fontGenerators.Add(fontGenerator);

        var cachePath = fontCacheDirectory;
        if (Directory.Exists(cachePath))
        {
            var path = Path.Combine(cachePath, HashHelper.GetMd5(fontGenerator.Directory) + ".yaml");
            if (File.Exists(path))
            {
                var cachedFontRoot = TinyToken.Read(path);
                if (cachedFontRoot != null)
                    fontGenerator.HandleCache(cachedFontRoot);
            }
        }

        return fontGenerator;
    }

    private void saveFontCache()
    {
        var cachePath = fontCacheDirectory;
        if (!Directory.Exists(cachePath))
            Directory.CreateDirectory(cachePath);

        foreach (var fontGenerator in fontGenerators)
        {
            var path = Path.Combine(cachePath, HashHelper.GetMd5(fontGenerator.Directory) + ".yaml");

            var fontRoot = fontGenerator.ToTinyObject();
            try
            {
                fontRoot.Write(path);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Failed to save font cache for {path} ({e.GetType().FullName})");
            }
        }
    }



    #endregion

    #region Configuration

    public void UpdateConfiguration(EffectConfig config)
    {
        if (context != null) throw new InvalidOperationException();

        var remainingFieldNames = new List<string>(config.FieldNames);
        foreach (var configurableField in configurableFields)
        {
            var field = configurableField.Field;
            NamedValue[] allowedValues = [];

            var fieldType = field.FieldType;
            if (fieldType.IsEnum)
            {
                var enumValues = Enum.GetValues(fieldType);
                fieldType = Enum.GetUnderlyingType(fieldType);

                allowedValues = new NamedValue[enumValues.Length];
                for (var i = 0; i < enumValues.Length; i++)
                {
                    var value = enumValues.GetValue(i);
                    allowedValues[i] = new NamedValue()
                    {
                        Name = value?.ToString() ?? throw new Exception(),
                        Value = Convert.ChangeType(value, fieldType, CultureInfo.InvariantCulture),
                    };
                }
            }

            try
            {
                var displayName = configurableField.Attribute.DisplayName;
                var initialValue = Convert.ChangeType(configurableField.InitialValue, fieldType, CultureInfo.InvariantCulture);
                config.UpdateField(field.Name, displayName, configurableField.Description, configurableField.Order, fieldType, initialValue, allowedValues, configurableField.BeginsGroup);

                var value = config.GetValue(field.Name);
                field.SetValue(this, value);

                remainingFieldNames.Remove(field.Name);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Failed to update configuration for {field.Name} with type {fieldType}:\n{e}");
            }
        }
        foreach (var name in remainingFieldNames)
            config.RemoveField(name);
    }

    public void ApplyConfiguration(EffectConfig config)
    {
        if (context != null) throw new InvalidOperationException();

        foreach (var configurableField in configurableFields)
        {
            var field = configurableField.Field;
            try
            {
                var value = config.GetValue(field.Name);
                field.SetValue(this, value);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Failed to apply configuration for {field.Name}:\n{e}");
            }
        }
    }

    private void initializeConfigurableFields()
    {
        configurableFields = [];

        var order = 0;
        var type = GetType();
        foreach (var field in type.GetFields())
        {
            var configurable = field.GetCustomAttribute<ConfigurableAttribute>(true);
            if (configurable == null)
                continue;

            if (!field.FieldType.IsEnum && !ObjectSerializer.Supports(field.FieldType.FullName ?? throw new Exception()))
                continue;

            var group = field.GetCustomAttribute<GroupAttribute>(true);
            var description = field.GetCustomAttribute<DescriptionAttribute>(true);

            configurableFields.Add(new ConfigurableField()
            {
                Field = field,
                Attribute = configurable,
                InitialValue = field.GetValue(this) ?? throw new Exception(),
                BeginsGroup = group?.Name?.Trim() ?? string.Empty,
                Description = description?.Content?.Trim() ?? string.Empty,
                Order = order++,
            });
        }
    }

    private struct ConfigurableField
    {
        public FieldInfo Field;
        public ConfigurableAttribute Attribute;
        public object InitialValue;
        public string BeginsGroup;
        public string Description;
        public int Order;

        public override string ToString() => $"{Field.Name} {InitialValue}";
    }

    #endregion

    public void Generate(GeneratorContext context)
    {
        if (Current != null) throw new InvalidOperationException("A script is already running in this domain");

        try
        {
            this.context = context;

            random = new Random(RandomSeed);

            Current = this;
            Generate();

            context.Multithreaded = Multithreaded;
            // saveFontCache();
        }
        finally
        {
            Current = null;

            foreach (var SKBitmap in bitmaps.Values)
                SKBitmap.Dispose();
            bitmaps.Clear();
        }
    }

    public abstract void Generate();
}

*/
