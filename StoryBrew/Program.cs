using StoryBrew.Storyboarding;

namespace StoryBrew;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("StoryBrew new <name> <mapset_path>");
            
            Console.WriteLine("StoryBrew build <path>");
            Console.WriteLine("StoryBrew clean <path>");
            Console.WriteLine("StoryBrew version");
            return;
        }

        Action handle = args[0].ToLower() switch
        {
            "new" when args.Length == 3 => () => New(args[1], args[2]),
            "build" when args.Length == 2 => () => Build(args[1]),
            "version" => Version,
            "clean" => () => Clean(args[1]),
            _ => () => Console.WriteLine("Invalid command or arguments.")
        };
        handle();
    }

    public static void New(string name, string path)
    {
        try
        {
            Project.Create(name, path);
            Console.WriteLine($"Project '{name}' created successfully at '{path}'.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create project: {ex}");
        }
    }

    public static void Build(string path)
    {
        try
        {
            var project = Project.Load(path);
            project.ExportToOsb();
            Console.WriteLine($"Project at '{path}' built successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to build project: {ex}");
        }
    }

    public static void Version()
    {
        try
        {
            var assembly = typeof(Program).Assembly;
            var assemblyName = assembly.GetName();
            Console.WriteLine($"{assemblyName.Name} {assemblyName.Version}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed get version: {ex}");
        }
    }
    
    public static void Clean(string path)
    {
        try
        {
            // TODO: similar to dotnet clean ?
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to Clean roject: {ex}");
        }
    }
}
