namespace StoryBrew.CLI;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            help();
            return;
        }

        static void help()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("StoryBrew new <project_path> <mapset_path>");
            Console.WriteLine("StoryBrew build <path>");
            Console.WriteLine("StoryBrew run <path>");
            Console.WriteLine("StoryBrew clean <path>");
            Console.WriteLine("StoryBrew add <path> <name>");
            Console.WriteLine("StoryBrew create <path> <name>");
            Console.WriteLine("StoryBrew version");
        }

        Action handler = args[0].ToLower() switch
        {
            "new" when args.Length == 3 => () => New(args[1], args[2]),
            "build" when args.Length == 2 => () => Build(args[1]),
            "run" when args.Length == 2 => () => Run(args[1]),
            "clean" => () => Clean(args[1]),
            "add" when args.Length == 3 => () => New(args[1], args[2]),
            "create" when args.Length == 3 => () => Create(args[1], args[2]),
            "help" => help,
            "version" => Version,
            _ => () => Console.WriteLine("Invalid command or arguments.")
        };
        handler();
    }

    public static void New(string projectPath, string mapsetPath)
    {
        try
        {
            Project.Manager.New(projectPath, mapsetPath);
            Console.WriteLine($"Created project at '{projectPath}' for mapset '{mapsetPath}' successfully.");
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
            var project = new Project.Manager(path);
            var result = project.Build(out var log) ? "Success" : "Failed";
            if (!string.IsNullOrEmpty(log)) Console.WriteLine($"{log}");
            Console.WriteLine($"Build {result}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to build project: {ex}");
        }
    }

    public static void Run(string path)
    {
        try
        {
            var project = new Project.Manager(path);
            var result = project.Run(out var log) ? "Success" : "Failed";
            if (!string.IsNullOrEmpty(log)) Console.WriteLine($"{log}");
            Console.WriteLine($"Run {result}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to run project: {ex}");
        }
    }

    public static void Clean(string path)
    {
        try
        {
            var project = new Project.Manager(path);
            project.Clean();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to Clean roject: {ex}");
        }
    }

    public static void Add(string path, string name)
    {
        try
        {
            // TODO: adds a script instance to the project config
            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to Add an script to the project: {ex}");
        }
    }

    public static void Create(string path, string name)
    {
        try
        {
            var project = new Project.Manager(path);
            project.Create(name);
            Console.WriteLine($"Created script '{name}' successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create an script to the project: {ex}");
        }
    }

    public static void Version()
    {
        Console.WriteLine($"StoryBrew version 1.0.0");
    }
}
