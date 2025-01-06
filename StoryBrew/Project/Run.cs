using System.Reflection;
using StoryBrew.Common.Scripting;

namespace StoryBrew;

public partial class Project
{
    public void Run()
    {
        Build();

        void runLayer(List<string> list)
        {
            foreach (var key in list)
            {
                if (!configuration.Instances.TryGetValue(key, out Script? instance)) throw new Exception($"Failed to find instance for {key}");

                Run(instance.ScriptFile, instance.Configurations);
            }
        }

        runLayer(configuration.Background);
        runLayer(configuration.Fail);
        runLayer(configuration.Pass);
        runLayer(configuration.Foreground);
        runLayer(configuration.Overlay);
        runLayer(configuration.Video);
    }

    public static void Run(string script, object args)
    {
        var instance = instantiate<StoryboardObjectGenerator>(script);
        instance.Generate();

        // File.WriteAllText(script, "");
    }

    private static T instantiate<T>(string filePath) where T : class
    {
        var name = Path.GetFileNameWithoutExtension(filePath);

        var assembly = Assembly.LoadFrom(filePath);
        var type = assembly.GetType(name) ?? throw new Exception($"Class {name} do not exists in assembly.");

        if (type is not T) throw new Exception($"Class {name} is not a {nameof(T)}.");

        return (T?)Activator.CreateInstance(type) ?? throw new Exception($"Failed to instantiate {nameof(T)} from assembly {filePath}.");
    }
}
