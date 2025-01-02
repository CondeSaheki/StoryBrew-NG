using StoryBrew.Viewer.Util;

namespace StoryBrew.Viewer;

class Program
{
    public static void Main()
    {
        const string FilePath = "test.txt";

        using var watcher = new SafeFileWatcher(FilePath, OnOsbUpdate);

        Console.WriteLine($"Monitoring file \"{FilePath}\"");
        Console.ReadLine(); // simulate work
    }

    private static void OnOsbUpdate(string? content, CancellationToken token)
    {
        // NOTE: must implement thread safety mecanisms for modification of external resources


        Console.WriteLine($"content: {content}");

        // Parse the file content

        token.ThrowIfCancellationRequested(); // Check for cancellation

        // Trigger rendering ?

        token.ThrowIfCancellationRequested(); // Check for cancellation
    }
}