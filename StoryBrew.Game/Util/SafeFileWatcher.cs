namespace StoryBrew.Viewer.Util;

public class SafeFileWatcher : IDisposable
{
    private CancellationTokenSource? tokenSource = null;
    private readonly FileSystemWatcher fileWatcher;
    private readonly object TaskLock = new();
    private static int TaskCount = 0;

    public readonly string FilePath;
    public readonly Action<string?, CancellationToken> Task;

    /// <summary>
    /// Initializes a new instance of the <see cref="SafeFileWatcher"/> class.
    /// Monitors the specified file for changes and safely triggers a task when changes occur.
    /// </summary>
    /// <param name="filePath">The path of the file to watch. Must be a valid existing file.</param>
    /// <param name="task">The task to execute when the file changes, with the file content
    /// and a cancellation token as parameters.</param>
    /// <exception cref="ArgumentException">Thrown when the file path is invalid or the file does not exist.</exception>
    public SafeFileWatcher(string filePath, Action<string?, CancellationToken> task)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            throw new ArgumentException("File path is invalid or file does not exist.", nameof(filePath));

        FilePath = filePath;
        Task = task;

        var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        var fileName = Path.GetFileName(filePath);

        fileWatcher = new FileSystemWatcher
        {
            Path = directory,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            Filter = fileName,
            EnableRaisingEvents = true
        };

        fileWatcher.Changed += OnChanged;
        fileWatcher.Created += OnChanged;
        fileWatcher.Deleted += OnChanged;

        Trigger();
    }

    private void OnChanged(object? _, FileSystemEventArgs? __)
    {
        // Ensure at max one pending task at a time.
        if(Volatile.Read(ref TaskCount) > 2) return;
        Interlocked.Increment(ref TaskCount);

        tokenSource?.Cancel();

        lock (TaskLock)
        {
            try
            {
                tokenSource = new CancellationTokenSource();

                string? content = null;
                if (File.Exists(FilePath))
                {
                    content = File.ReadAllText(FilePath);
                    tokenSource.Token.ThrowIfCancellationRequested();
                }
                Task.Invoke(content, tokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SafeFileWatcher, FilePath \"{FilePath}\": Error during task execution {ex}");
            }
            finally
            {
                tokenSource?.Dispose();
                tokenSource = null;

                Interlocked.Decrement(ref TaskCount);
            }
        }
    }

    /// <summary>
    /// Manually trigger the file change task.
    /// </summary>
    public void Trigger()
    {
        OnChanged(null, null);
    }
    
    /// <summary>
    /// Cancels All ongoing tasks.
    /// </summary>
    public void Cancel()
    {
        do
        {
            tokenSource?.Cancel();
            
            lock (TaskLock) { } // TODO: add a timeout for this lock obtention
        }
        while (Volatile.Read(ref TaskCount) != 0);
    }
    
    public void Dispose()
    { 
        fileWatcher.Dispose();
        Cancel();
    }
}
