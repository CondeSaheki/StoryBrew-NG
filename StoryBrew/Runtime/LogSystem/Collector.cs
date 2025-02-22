using System.Text;

namespace StoryBrew.Runtime.LogSystem;

internal class Collector : IDisposable
{
    private StringBuilder builder;

    public Collector()
    {
        builder = new();
        Log.Event += onLog;
    }

    private void onLog(object? _, (Level level, string content) arguments) => builder.AppendLine($"{arguments.level}: {arguments.content}");

    public string Consume()
    {
        Log.Event -= onLog;
        return builder.ToString();
    }

    public void Dispose()
    {
        Log.Event -= onLog; // Note: Unsubscribing the handler multiple times is fine
    }
}