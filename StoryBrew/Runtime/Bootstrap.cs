using StoryBrew.Runtime.LogSystem;

namespace StoryBrew.Runtime;

public partial class Bootstrap
{
    internal static readonly Version VERSION = new(1, 0, 0);

    public delegate Type? GetScriptTypeDelegate(ReadOnlySpan<char> name);
    public delegate ReadOnlySpan<Type> GetScriptsDelegate();
    public delegate ReadOnlySpan<char> GetBuildIdDelegate();
    private readonly GetScriptTypeDelegate getScriptType;
    private readonly GetScriptsDelegate getScripts;
    private readonly GetBuildIdDelegate getBuildId;

    public Bootstrap(GetScriptsDelegate getScriptsDelegate, GetScriptTypeDelegate getScriptTypeDelegate, GetBuildIdDelegate getBuildIdDelegate)
    {
        getScripts = getScriptsDelegate;
        getScriptType = getScriptTypeDelegate;
        getBuildId = getBuildIdDelegate;
    }

    public void Arguments(ReadOnlySpan<string> arguments)
    {
        try
        {
            if (arguments.Length == 0)
            {
                handleHelpCommand();
                return;
            }

            switch (arguments[0])
            {
                case "pipe" when arguments.Length == 1: handlePipeCommand(); break;
                case "new" when arguments.Length <= 3 && arguments.Length >= 1: handleNewCommand(arguments[1..]); break;
                case "run" when arguments.Length == 2: handleRunCommand(arguments[1]); break;
                case "help" when arguments.Length == 1: handleHelpCommand(); break;
                case "version" when arguments.Length == 1: handleVersionCommand(); break;

                default:
                    Log.Error("Invalid command or arguments.");
                    handleHelpCommand();
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
        }  
    }

    private static void handleHelpCommand() => Log.Message(
    $$"""
    Usage: command [options]

    Commands:
        pipe                   Attempt to connect to the StoryBrew Editor pipe server.
        new <file_path>        Creates a new configuration in the specified target.
        run <file_path>        Run the specified target configurations.
        help                   Displays this help message.
        version                Shows the current version.
    """);
}