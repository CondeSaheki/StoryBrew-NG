using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace StoryBrew.Runtime.LogSystem;

internal static class Log
{
    public const bool TO_EVENT = true;
    public const bool TO_FILE = true;
    public const bool TO_CONSOLE = true;

    public static Level LogLevel = Level.Debug;
    public static string FILE_PATH = "StoryBrew.log"; // Todo: Make the file path gets automatically generated based in the user OS temp folder

    public static void Message(in string content) => write(Level.Message, content);
    public static void Error(in string content) => write(Level.Error, content);
    public static void Warnning(in string content) => write(Level.Warnning, content);
    public static void Debug(in string content) => write(Level.Debug, content);
    public static void User(in string content) => write(Level.User, content);

    public static event EventHandler<(Level level, string content)> Event = delegate { };
    private static Lock writeLock = new();

    private static void write(Level level, in string content)
    {
        lock (writeLock)
        {
            if ((int)level > (int)LogLevel) return;

            if (TO_FILE) fileWrite(level, content);
            if (TO_CONSOLE) consoleWrite(level, content);
            if (TO_EVENT) memoryWrite(level, content);
        }
    }

    private static void fileWrite(Level level, ReadOnlySpan<char> content)
    {
        const string mutex_name = "Global\\storybrew_log_mutex"; // Note: "Global\\" is needed for windows compatibility :clown: 

        using Mutex mutex = new(false, mutex_name);
        mutex.WaitOne();
        try
        {
            using FileStream fileStream = new(FILE_PATH, FileMode.Append, FileAccess.Write, FileShare.Read);
            using StreamWriter streamWriter = new(fileStream, Encoding.UTF8);
            streamWriter.WriteLine(syslogFormat(level, content));
            streamWriter.Flush();
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }

    private static string syslogFormat(Level level, ReadOnlySpan<char> content)
    {
        var timestamp = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        var host = Environment.MachineName;
        var process = Process.GetCurrentProcess().ProcessName;
        int processId = Environment.ProcessId;

        return $"{timestamp} {host} {process}[{processId}]: {level}: {content}";
    }

    private static void consoleWrite(Level level, in string content)
    {
        switch (level)
        {
            case Level.Error: Console.ForegroundColor = ConsoleColor.Red; break;
            case Level.Warnning: Console.ForegroundColor = ConsoleColor.Yellow; break;
            case Level.Debug: Console.ForegroundColor = ConsoleColor.Blue; break;
            default: break;
        }

        Console.WriteLine(content);
        Console.ResetColor();
    }

    private static void memoryWrite(Level level, in string content) => Event.Invoke(null, (level, content));

}

internal enum Level
{
    Nothing = -1,
    Error,
    Warnning,
    Message,
    User,
    Debug
}