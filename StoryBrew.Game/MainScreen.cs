// Note: this file needs a complete refactor. I am not used to work the osu!framework so it is a mess.

using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Screens;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.StoryboardsNG;
using osuTK.Graphics;
using StoryBrew.Runtime;
using StoryBrew.Runtime.Pipe;

namespace StoryBrew.Game;

public partial class MainScreen : Screen
{
    private readonly Runtime.Version currentVersion = new(1, 0, 0);
    private CancellationTokenSource tokenSource = null!;

    private ProjectData? project = null;
    private string? hash = null;
    private string? buildId = null;
    private Dictionary<string, JSchema>? schemas = null;
    private string? storyboardRaw = null;

    [BackgroundDependencyLoader]
    private void load()
    {
        // Todo: Make a Project creation/loading screen

        tokenSource = new();

        // Note: This is a mock for testing
        project = ProjectData.FromFile("Configuration.json");

        Task.Run(() => pipe(tokenSource.Token), tokenSource.Token);

        InternalChildren =
        [
            new FillFlowContainer
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                AutoSizeAxes = Axes.Both,
                Children =
                [
                    new BasicButton
                    {
                        AutoSizeAxes = Axes.Both,
                        Text = "Update",
                        Action = () => Task.Run(dotnetRun)
                    },
                ]
            }
        ];
    }

    private void dotnetRun()
    {
        // Todo: Make a lock for this operation
        if (project == null) return;

        Console.WriteLine("dotnet" + $" run \"{project.DirectoryPath}\" -- pipe");

        var config = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run -c Release --project \"{project.DirectoryPath}\" -- pipe",
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(config) ?? throw new Exception("Failed to start dotnet process.");

        process.WaitForExit();
    }


    private void updateStoryboard()
    {
        Console.WriteLine("update called");
        if (project == null || storyboardRaw == null) return;

        var osbDecoder = new LegacyStoryboardDecoderRE();

        using MemoryStream stream = new(Encoding.UTF8.GetBytes(storyboardRaw));

        // Note: This is a mock for testing
        // using FileStream stream = new("/home/saheki/osu/Songs/Various Artists - Anime 2024 Mix/Various Artists - Anime 2024 Mix (Net0)-----------------------------------VIDEO.osb",
        // FileMode.Open, FileAccess.Read, FileShare.Read);

        using LineBufferedReader reader = new(stream);
        var storyboard = osbDecoder.Decode(reader);

        InternalChildren =
        [
            storyboard.CreateDrawable(project.MapsetDirectoryPath),
            new FillFlowContainer
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                AutoSizeAxes = Axes.Both,
                Children =
                [
                    new BasicButton
                    {
                        AutoSizeAxes = Axes.Both,
                        Text = "Update",
                        Action = () => Task.Run(dotnetRun)
                    },
                ]
            }
        ];
        Console.WriteLine("update complete");
    }

    protected override void Dispose(bool isDisposing)
    {
        tokenSource?.Cancel();
        tokenSource?.Dispose();
        base.Dispose(isDisposing);
    }

    private void pipe(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using Server server = new();
            try
            {
                if (!server.TryRequest(new(Content.Version), out var versionResponse, cancellationToken))
                {
                    // Todo: Ask user if they want to update their project
                    continue;
                }

                var version = JsonConvert.DeserializeObject<VersionInfo>(versionResponse);

                if (version.Version != currentVersion) server.Close("Version does not match");

                if (version.Hash != hash || hash == null)
                {
                    if (!server.TryRequest(new(Content.Schema), out var schemaResponse, cancellationToken))
                    {
                        // Todo: Tell the user that the schema failed to be generated
                        continue;
                    }

                    schemas = parseSchemas(schemaResponse);
                    hash = version.Hash;
                    buildId = null;

                    // Todo: ask user to update their configuration

                    // Note: next comments should be the behavior of the server, commented for testing.
                    // server.Close();
                    // continue;
                }

                // Todo: verify if configurations match saved schema

                if (version.BuildId == buildId && buildId != null)
                {
                    // Todo: Ask user if they want to run it anyway
                    continue;
                }

                if (!server.TryRequest(new(Content.StoryBoard, project?.ToJsonString() ?? ""), out var storyBoardResponse, cancellationToken))
                {
                    // Todo: Tell the user that the StoryBoard failed to be generated
                    continue;
                }

                storyboardRaw = storyBoardResponse;
                buildId = version.BuildId;

                // Note: Not sure if it should be done here, I do not know how to handle thread safety in osu!framework.
                Schedule(updateStoryboard);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpect exception {ex}");
            }
            finally
            {
                server.Close();
            }
        }
    }

    // Todo: move this to runtime
    private static Dictionary<string, JSchema> parseSchemas(string schemaString)
    {
        JObject json = JObject.Parse(schemaString);

        Dictionary<string, JSchema> schemas = [];

        foreach (var property in json.Properties())
        {
            schemas[property.Name] = JSchema.Parse(property.Value.ToString());
        }
        return schemas;
    }
}
