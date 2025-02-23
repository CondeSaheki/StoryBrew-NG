using StoryBrew.Runtime.LogSystem;

namespace StoryBrew.Runtime;

public partial class Bootstrap
{
    private void handleNewCommand(ReadOnlySpan<string> arguments)
    {
        if (arguments.Length == 0)
        {
            handleNewHelpCommand();
            return;
        }
        switch (arguments[0])
        {
            case "project": handleNewConfigurationCommand(arguments[1]); break;
            case "schema": handleNewSchemaCommand(arguments[1]); break;
            default:
                Log.Error("Invalid command or arguments.");
                handleHelpCommand();
                break;
        }
    }

    private static void handleNewHelpCommand() => Log.Message(
        """
        Avaliable templates:
            project                project configuration json file
            schema                 scripts schema json file
        """
    );

    private void handleNewConfigurationCommand(in string directoryPath)
    {
        const string file_name = "Configuration.json";

        var filePath = Path.Combine(directoryPath, file_name);

        if (File.Exists(filePath))
        {
            Log.Error($"Configuration.json already exists in {directoryPath}");
            return;
        }

        Directory.CreateDirectory(directoryPath);

        ProjectData project = new();
        project.Save(filePath);
    }

    private void handleNewSchemaCommand(in string directoryPath)
    {
        const string file_name = "Schema.json";

        var filePath = Path.Combine(directoryPath, file_name);

        if (File.Exists(filePath))
        {
            Log.Error($"configuration.json already exists in {directoryPath}");
            return;
        }

        Directory.CreateDirectory(directoryPath);

        using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write);
        using StreamWriter streamWriter = new(fileStream);
        streamWriter.Write(getSchema());
    }
}