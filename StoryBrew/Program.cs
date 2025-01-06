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
            Console.WriteLine("StoryBrew run <path>");
            Console.WriteLine("StoryBrew clean <path>");
            Console.WriteLine("StoryBrew add <path> <name>");
            Console.WriteLine("StoryBrew version");
            return;
        }

        Action handler = args[0].ToLower() switch
        {
            "new" when args.Length == 3 => () => New(args[1], args[2]),
            "build" when args.Length == 2 => () => Build(args[1]),
            "run" when args.Length == 2 => () => Build(args[1]),
            "clean" => () => Clean(args[1]),
            "add" when args.Length == 3 => () => New(args[1], args[2]),
            "version" => Version,
            _ => () => Console.WriteLine("Invalid command or arguments.")
        };
        handler();
    }

    public static void New(string name, string path)
    {
        try
        {
            Project.New(name, path);
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
            var project = new Project(path);
            project.Build();
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
            var project = new Project(path);
            project.Run();
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
            var project = new Project(path);
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
            // TODO: adds a script template to the project
            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to Add an script to the project: {ex}");
        }
    }

    public static void Version()
    {
        Console.WriteLine($"StoryBrew version 1.0.0");
    }
}
