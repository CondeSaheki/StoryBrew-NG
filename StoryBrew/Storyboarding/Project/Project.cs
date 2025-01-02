using StoryBrew.Util;
using StoryBrew.Common.Scripting;
using StoryBrew.Common.Storyboarding;
using StoryBrew.Common.Util;
using StoryBrew.Mapset;
using StoryBrew.Scripting;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Tiny;

namespace StoryBrew.Storyboarding;

public partial class Project : IDisposable
{
    public static readonly Encoding Encoding = new UTF8Encoding();

    public const string BinaryExtension = ".sbp";
    public const string TextExtension = ".yaml";
    public const string DefaultBinaryFilename = "project" + BinaryExtension;
    public const string DefaultTextFilename = "project.sbrew" + TextExtension;
    public const string DataFolder = ".sbrew";
    public const string ProjectsFolder = "projects";

    public const string FileFilter = "project files|" + DefaultBinaryFilename + ";" + DefaultTextFilename;

    private ScriptManager<StoryboardObjectGenerator>? scriptManager;

    public string ProjectPath;
    public string ProjectFolderPath => Path.GetDirectoryName(ProjectPath) ?? string.Empty;
    public string ProjectAssetFolderPath => Path.Combine(ProjectFolderPath, "assetlibrary");

    public string? ScriptsPath { get; }
    public string? CommonScriptsPath { get; }
    public string ScriptsLibraryPath { get; }

    public string? AudioPath
    {
        get
        {
            if (!Directory.Exists(MapsetPath))
                return null;

            foreach (var beatmap in MapsetManager.Beatmaps)
            {
                if (beatmap.AudioFilename == null)
                    continue;

                var path = Path.Combine(MapsetPath, beatmap.AudioFilename);
                if (!File.Exists(path))
                    continue;

                return path;
            }

            return Directory.GetFiles(MapsetPath, "*.mp3", SearchOption.TopDirectoryOnly).FirstOrDefault() ?? string.Empty;
        }
    }

    public string OsbPath
    {
        get
        {
            if (!MapsetPathIsValid)
                return Path.Combine(ProjectFolderPath, "storyboard.osb");

            // Find the correct osb filename from .osu files
            var regex = new Regex(@"^(.+ - .+ \(.+\)) \[.+\].osu$");
            foreach (var osuFilePath in Directory.GetFiles(MapsetPath, "*.osu", SearchOption.TopDirectoryOnly))
            {
                var osuFilename = Path.GetFileName(osuFilePath);

                Match match;
                if ((match = regex.Match(osuFilename)).Success)
                    return Path.Combine(MapsetPath, $"{match.Groups[1].Value}.osb");
            }

            // Use an existing osb
            foreach (var osbFilePath in Directory.GetFiles(MapsetPath, "*.osb", SearchOption.TopDirectoryOnly))
                return osbFilePath;

            // Whatever
            return Path.Combine(MapsetPath, "storyboard.osb");
        }
    }

    public const int Version = 7;

    private bool ownsOsb;
    public bool OwnsOsb
    {
        get { return ownsOsb; }
        set
        {
            if (ownsOsb == value) return;
            ownsOsb = value;
        }
    }

    public static readonly OsbLayer[] OsbLayers = (OsbLayer[])Enum.GetValues(typeof(OsbLayer));

    public double DisplayTime;

    public float DimFactor;

    public readonly ExportSettings ExportSettings = new ExportSettings();

    public LayerManager LayerManager { get; } = new LayerManager();

    public Project(string projectPath, bool withCommonScripts)
    {
        ProjectPath = projectPath;

        ScriptsPath = Path.GetDirectoryName(projectPath) ?? string.Empty;
        if (withCommonScripts)
        {
            CommonScriptsPath = Path.GetFullPath(Path.Combine("..", "..", "..", "scripts"));
            if (!Directory.Exists(CommonScriptsPath))
            {
                CommonScriptsPath = Path.GetFullPath("scripts");
                if (!Directory.Exists(CommonScriptsPath))
                    Directory.CreateDirectory(CommonScriptsPath);
            }
        }
        ScriptsLibraryPath = Path.Combine(ScriptsPath, "scriptslibrary");
        if (!Directory.Exists(ScriptsLibraryPath))
            Directory.CreateDirectory(ScriptsLibraryPath);

        Console.WriteLine($"Scripts path - project:{ScriptsPath}, common:{CommonScriptsPath}, library:{ScriptsLibraryPath}");

        var compiledScriptsPath = Path.GetFullPath("cache/scripts");
        if (!Directory.Exists(compiledScriptsPath))
            Directory.CreateDirectory(compiledScriptsPath);
        else
        {
            cleanupFolder(compiledScriptsPath, "*.dll");
            cleanupFolder(compiledScriptsPath, "*.pdb");
        }

        scriptManager = new ScriptManager<StoryboardObjectGenerator>("StorybrewScripts", ScriptsPath, CommonScriptsPath, ScriptsLibraryPath, compiledScriptsPath, ReferencedAssemblies);

        foreach (var effect in Effects) QueueEffectUpdate(effect);
    }

    private static readonly Regex effectGuidRegex = new Regex("effect\\.([a-z0-9]{32})\\.yaml", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public void Save()
    {
        saveText(ProjectPath.Replace(DefaultBinaryFilename, DefaultTextFilename));
    }

    public static Project Load(string projectPath, bool withCommonScripts = true)
    {
        // Binary format isn't saved anymore and may be obsolete:
        // Load from the text format if possible even if the binary format has been selected.
        var textFormatPath = projectPath.Replace(DefaultBinaryFilename, DefaultTextFilename);
        if (projectPath.EndsWith(BinaryExtension) && File.Exists(textFormatPath)) projectPath = textFormatPath;

        var project = new Project(projectPath, withCommonScripts);
        if (projectPath.EndsWith(BinaryExtension)) project.loadBinary(projectPath);
        else project.loadText(textFormatPath);
        return project;
    }

    private void loadBinary(string path)
    {
        using (var stream = new FileStream(path, FileMode.Open))
        using (var r = new BinaryReader(stream, Encoding.UTF8))
        {
            var version = r.ReadInt32();
            if (version > Version)
                throw new InvalidOperationException("This project was saved with a more recent version, you need to update to open it");

            var savedBy = r.ReadString();
            Debug.Print($"Loading project saved by {savedBy}");

            MapsetPath = r.ReadString();
            if (version >= 1)
            {
                var mainBeatmapId = r.ReadInt64();
                var mainBeatmapName = r.ReadString();
                SelectBeatmap(mainBeatmapId, mainBeatmapName);
            }

            OwnsOsb = version >= 4 ? r.ReadBoolean() : true;

            var effectCount = r.ReadInt32();
            for (int effectIndex = 0; effectIndex < effectCount; effectIndex++)
            {
                var guid = version >= 6 ? new Guid(r.ReadBytes(16)) : Guid.NewGuid();
                var baseName = r.ReadString();
                var name = r.ReadString();

                var effect = AddScriptedEffect(baseName);
                effect.Guid = guid;
                effect.Name = name;

                if (version >= 1)
                {
                    var fieldCount = r.ReadInt32();
                    for (int fieldIndex = 0; fieldIndex < fieldCount; fieldIndex++)
                    {
                        var fieldName = r.ReadString();
                        var fieldDisplayName = r.ReadString();
                        var fieldValue = ObjectSerializer.Read(r);

                        var allowedValueCount = r.ReadInt32();
                        var allowedValues = allowedValueCount > 0 ? new NamedValue[allowedValueCount] : null;
                        for (int allowedValueIndex = 0; allowedValueIndex < allowedValueCount; allowedValueIndex++)
                        {
                            var allowedValueName = r.ReadString();
                            var allowedValue = ObjectSerializer.Read(r);
                            if (allowedValues == null) throw new Exception("");
                            allowedValues[allowedValueIndex] = new NamedValue()
                            {
                                Name = allowedValueName,
                                Value = allowedValue ?? throw new Exception(""),
                            };
                        }
                        effect.Config.UpdateField(fieldName, fieldDisplayName, null, fieldIndex, fieldValue?.GetType(), fieldValue, allowedValues, null);
                    }
                }
            }

            var layerCount = r.ReadInt32();
            for (var layerIndex = 0; layerIndex < layerCount; layerIndex++)
            {
                var guid = version >= 6 ? new Guid(r.ReadBytes(16)) : Guid.NewGuid();
                var identifier = r.ReadString();
                var effectIndex = r.ReadInt32();
                var diffSpecific = version >= 3 ? r.ReadBoolean() : false;
                var osbLayer = version >= 2 ? (OsbLayer)r.ReadInt32() : OsbLayer.Background;
                var visible = r.ReadBoolean();

                var effect = Effects[effectIndex];
                effect.AddPlaceholder(new EditorStoryboardLayer(identifier, effect)
                {
                    Guid = guid,
                    DiffSpecific = diffSpecific,
                    OsbLayer = osbLayer,
                    Visible = visible,
                });
            }

            if (version >= 5)
            {
                var assemblyCount = r.ReadInt32();
                var importedAssemblies = new List<string>();
                for (var assemblyIndex = 0; assemblyIndex < assemblyCount; assemblyIndex++)
                {
                    var assembly = r.ReadString();
                    importedAssemblies.Add(assembly);
                }
                ImportedAssemblies = importedAssemblies;
            }
        }
    }

    private void saveText(string path)
    {

        // Create the opener file
        if (!File.Exists(path))
            File.WriteAllText(path, "# This file is only used to open the project\n# Project data is contained in the .sbrew folder");

        var projectDirectory = Path.GetDirectoryName(path) ?? string.Empty;

        var gitIgnorePath = Path.Combine(projectDirectory, ".gitignore");
        if (!File.Exists(gitIgnorePath))
            File.WriteAllText(gitIgnorePath, ".sbrew/user.yaml\n.sbrew.tmp\n.sbrew.bak\n.cache\n.vs");

        var targetDirectory = Path.Combine(projectDirectory, DataFolder);
        using (var directoryWriter = new SafeDirectoryWriter(targetDirectory))
        {
            // Write the index
            {
                var indexRoot = new TinyObject
                    {
                        { "FormatVersion", Version },
                        { "BeatmapId", MainBeatmap.Id },
                        { "BeatmapName", MainBeatmap.Name },
                        { "Assemblies", importedAssemblies },
                        { "Layers", LayerManager.Layers.Select(l => l.Guid.ToString("N")) },
                    };

                var indexPath = directoryWriter.GetPath("index.yaml");
                indexRoot.Write(indexPath);
            }

            // Write user specific data
            {
                var userRoot = new TinyObject
                    {
                        { "FormatVersion", Version },
                        { "Editor", "??" },
                        { "MapsetPath", PathHelper.WithStandardSeparators(MapsetPath) },
                        { "ExportTimeAsFloatingPoint", ExportSettings.UseFloatForTime },
                        { "OwnsOsb", OwnsOsb },
                    };

                var userPath = directoryWriter.GetPath("user.yaml");
                userRoot.Write(userPath);
            }

            // Write each effect
            foreach (var effect in Effects)
            {
                var effectRoot = new TinyObject
                    {
                        { "FormatVersion", Version },
                        { "Name", effect.Name },
                        { "Script", effect.BaseName },
                        { "Multithreaded", effect.Multithreaded },
                    };

                var configRoot = new TinyObject();
                effectRoot.Add("Config", configRoot);

                foreach (var field in effect.Config.SortedFields)
                {
                    var fieldRoot = new TinyObject
                        {
                            { "Type", field.Type.FullName },
                            { "Value", ObjectSerializer.ToString(field.Type, field.Value)},
                        };
                    if (field.DisplayName != field.Name)
                        fieldRoot.Add("DisplayName", field.DisplayName);
                    if (!string.IsNullOrWhiteSpace(field.BeginsGroup))
                        fieldRoot.Add("BeginsGroup", field.BeginsGroup);
                    configRoot.Add(field.Name, fieldRoot);

                    if ((field.AllowedValues?.Length ?? 0) > 0)
                    {
                        var allowedValuesRoot = new TinyObject();
                        fieldRoot.Add("AllowedValues", allowedValuesRoot);

                        if (field.AllowedValues != null)
                        {
                            foreach (var allowedValue in field.AllowedValues)
                                allowedValuesRoot.Add(allowedValue.Name, ObjectSerializer.ToString(field.Type, allowedValue));
                        }
                    }
                }

                var layersRoot = new TinyObject();
                effectRoot.Add("Layers", layersRoot);

                foreach (var layer in LayerManager.Layers.Where(l => l.Effect == effect))
                {
                    var layerRoot = new TinyObject
                        {
                            { "Name", layer.Identifier },
                            { "OsbLayer", layer.OsbLayer },
                            { "DiffSpecific", layer.DiffSpecific },
                            { "Visible", layer.Visible },
                        };
                    layersRoot.Add(layer.Guid.ToString("N"), layerRoot);
                }

                var effectPath = directoryWriter.GetPath("effect." + effect.Guid.ToString("N") + ".yaml");
                effectRoot.Write(effectPath);
            }

            directoryWriter.Commit(checkPaths: true);
        }
    }

    private void loadText(string path)
    {
        var targetDirectory = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, DataFolder);
        using (var directoryReader = new SafeDirectoryReader(targetDirectory))
        {
            var indexPath = directoryReader.GetPath("index.yaml");
            var indexRoot = TinyToken.Read(indexPath);

            var indexVersion = indexRoot.Value<int>("FormatVersion");
            if (indexVersion > Version)
                throw new InvalidOperationException("This project was saved with a more recent version, you need to update to open it");

            var userPath = directoryReader.GetPath("user.yaml");
            TinyToken? userRoot = null;
            if (File.Exists(userPath))
            {
                userRoot = TinyToken.Read(userPath);

                var userVersion = userRoot.Value<int>("FormatVersion");
                if (userVersion > Version)
                    throw new InvalidOperationException("This project's user settings were saved with a more recent version, you need to update to open it");

                var savedBy = userRoot.Value<string>("Editor");
                Debug.Print($"Project saved by {savedBy}");

                ExportSettings.UseFloatForTime = userRoot.Value<bool>("ExportTimeAsFloatingPoint");
                OwnsOsb = userRoot.Value<bool>("OwnsOsb");
            }

            MapsetPath = userRoot?.Value<string>("MapsetPath") ?? indexRoot.Value<string>("MapsetPath") ?? "nul";
            SelectBeatmap(indexRoot.Value<long>("BeatmapId"), indexRoot.Value<string>("BeatmapName"));
            ImportedAssemblies = indexRoot.Values<string>("Assemblies");

            // Load effects
            var layerInserters = new Dictionary<string, Action>();
            foreach (var effectPath in Directory.EnumerateFiles(directoryReader.Path, "effect.*.yaml", SearchOption.TopDirectoryOnly))
            {
                var guidMatch = effectGuidRegex.Match(effectPath);
                if (!guidMatch.Success || guidMatch.Groups.Count < 2)
                    throw new InvalidDataException($"Could not parse effect Guid from '{effectPath}'");

                var effectRoot = TinyToken.Read(effectPath);

                var effectVersion = effectRoot.Value<int>("FormatVersion");
                if (effectVersion > Version)
                    throw new InvalidOperationException("This project contains an effect that was saved with a more recent version, you need to update to open it");

                var effect = AddScriptedEffect(effectRoot.Value<string>("Script"), effectRoot.Value<bool>("Multithreaded"));
                effect.Guid = Guid.Parse(guidMatch.Groups[1].Value);
                effect.Name = effectRoot.Value<string>("Name");

                var configRoot = effectRoot.Value<TinyObject>("Config");
                var fieldIndex = 0;
                foreach (var fieldProperty in configRoot)
                {
                    var fieldRoot = fieldProperty.Value;

                    var fieldTypeName = fieldRoot.Value<string>("Type");
                    var fieldContent = fieldRoot.Value<string>("Value");
                    var beginsGroup = fieldRoot.Value<string>("BeginsGroup");

                    var fieldValue = ObjectSerializer.FromString(fieldTypeName, fieldContent);

                    var allowedValues = fieldRoot
                            .Value<TinyObject>("AllowedValues")?
                            .Select(p => new NamedValue { Name = p.Key, Value = ObjectSerializer.FromString(fieldTypeName, p.Value.Value<string>()), })
                            .ToArray();

                    effect.Config.UpdateField(fieldProperty.Key, fieldRoot.Value<string>("DisplayName"), null, fieldIndex++, fieldValue?.GetType(), fieldValue, allowedValues, beginsGroup);
                }

                var layersRoot = effectRoot.Value<TinyObject>("Layers");
                foreach (var layerProperty in layersRoot)
                {
                    var layerEffect = effect;
                    var layerGuid = layerProperty.Key;
                    var layerRoot = layerProperty.Value;
                    layerInserters.Add(layerGuid, () => layerEffect.AddPlaceholder(new EditorStoryboardLayer(layerRoot.Value<string>("Name"), layerEffect)
                    {
                        Guid = Guid.Parse(layerGuid),
                        OsbLayer = layerRoot.Value<OsbLayer>("OsbLayer"),
                        DiffSpecific = layerRoot.Value<bool>("DiffSpecific"),
                        Visible = layerRoot.Value<bool>("Visible"),
                    }));
                }
            }

            // Insert layers defined in the index
            var layersOrder = indexRoot.Values<string>("Layers");
            if (layersOrder != null)
                foreach (var layerGuid in layersOrder.Distinct())
                {
                    if (layerInserters.TryGetValue(layerGuid, out var insertLayer))
                        insertLayer();
                }

            // Insert all remaining layers
            foreach (var key in layersOrder == null ? layerInserters.Keys : layerInserters.Keys.Except(layersOrder))
            {
                var insertLayer = layerInserters[key];
                insertLayer();
            }
        }
    }

    public static Project Create(string projectFolderName, string mapsetPath, bool withCommonScripts = true)
    {
        if (!Directory.Exists(ProjectsFolder))
            Directory.CreateDirectory(ProjectsFolder);

        var hasInvalidCharacters = false;
        foreach (var character in Path.GetInvalidFileNameChars())
        {

            if (projectFolderName.Contains(character.ToString()))
            {
                hasInvalidCharacters = true;
                break;
            }
        }

        if (hasInvalidCharacters || string.IsNullOrWhiteSpace(projectFolderName)) throw new InvalidOperationException($"'{projectFolderName}' isn't a valid project folder name");

        var projectFolderPath = Path.Combine(ProjectsFolder, projectFolderName);
        if (Directory.Exists(projectFolderPath)) throw new InvalidOperationException($"A project already exists at '{projectFolderPath}'");

        Directory.CreateDirectory(projectFolderPath);
        var project = new Project(Path.Combine(projectFolderPath, DefaultTextFilename), withCommonScripts)
        {
            MapsetPath = mapsetPath,
        };
        project.Save();

        return project;
    }

    public void ExportToOsb(bool exportOsb = true)
    {
        string? osuPath = null, osbPath = null;
        List<EditorStoryboardLayer>? localLayers = null;

        osuPath = MainBeatmap.Path;
        osbPath = OsbPath;

        if (!OwnsOsb && File.Exists(osbPath)) File.Copy(osbPath, $"{osbPath}.bak");
        OwnsOsb = true;

        localLayers = new List<EditorStoryboardLayer>(LayerManager.FindLayers(l => l.Visible));

        var usesOverlayLayer = localLayers.Any(l => l.OsbLayer == OsbLayer.Overlay);

        if (!string.IsNullOrEmpty(osuPath))
        {
            Console.WriteLine($"Exporting diff specific events to {osuPath}");
            using var stream = new SafeWriteStream(osuPath);
            using var writer = new StreamWriter(stream, Encoding);
            using var fileStream = new FileStream(osuPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(fileStream, Encoding);

            string? line;
            var inEvents = false;
            var inStoryboard = false;
            while ((line = reader.ReadLine()) != null)
            {
                var trimmedLine = line.Trim();
                if (!inEvents && trimmedLine == "[Events]")
                    inEvents = true;
                else if (trimmedLine.Length == 0)
                    inEvents = false;

                if (inEvents)
                {
                    if (trimmedLine.StartsWith("//Storyboard Layer"))
                    {
                        if (!inStoryboard)
                        {
                            foreach (var osbLayer in OsbLayers)
                            {
                                if (osbLayer == OsbLayer.Overlay && !usesOverlayLayer)
                                    continue;

                                writer.WriteLine($"//Storyboard Layer {(int)osbLayer} ({osbLayer})");
                                foreach (var layer in localLayers)
                                    if (layer.OsbLayer == osbLayer && layer.DiffSpecific)
                                        layer.WriteOsb(writer, ExportSettings);
                            }
                            inStoryboard = true;
                        }
                    }
                    else if (inStoryboard && trimmedLine.StartsWith("//"))
                        inStoryboard = false;

                    if (inStoryboard)
                        continue;
                }
                writer.WriteLine(line);
            }
            stream.Commit();
        }

        if (exportOsb)
        {
            Debug.Print($"Exporting osb to {osbPath}");

            using var stream = new SafeWriteStream(osbPath);
            using var writer = new StreamWriter(stream, Encoding);

            writer.WriteLine("[Events]");
            writer.WriteLine("//Background and Video events");

            foreach (var osbLayer in OsbLayers)
            {
                if (osbLayer == OsbLayer.Overlay && !usesOverlayLayer) continue;

                writer.WriteLine($"//Storyboard Layer {(int)osbLayer} ({osbLayer})");
                foreach (var layer in localLayers)
                {
                    if (layer.OsbLayer == osbLayer && !layer.DiffSpecific) layer.WriteOsb(writer, ExportSettings);
                }
            }
            writer.WriteLine("//Storyboard Sound Samples");
            stream.Commit();
        }
    }

    private static void cleanupFolder(string path, string searchPattern)
    {
        foreach (var filename in Directory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly))
            try
            {
                File.Delete(filename);
                Debug.Print($"{filename} deleted");
            }
            catch (Exception e)
            {
                Trace.WriteLine($"{filename} couldn't be deleted: {e.Message}");
            }
    }

    public void Dispose()
    {
        scriptManager?.Dispose();
        scriptManager = null;
    }
}
