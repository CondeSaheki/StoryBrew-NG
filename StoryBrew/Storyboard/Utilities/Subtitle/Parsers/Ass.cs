using System.Text;

namespace StoryBrew.Storyboard.Utilities.Subtitle.Parsers;

public static class Ass
{
    public static Set Parse(string path)
    {
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) return Parse(stream);
    }

    public static Set Parse(Stream stream)
    {
        var lines = new List<Line>();
        using (var reader = new StreamReader(stream, Encoding.UTF8))
            reader.ParseSections(sectionName =>
            {
                switch (sectionName)
                {
                    case "Events":
                        reader.ParseKeyValueSection((key, value) =>
                        {
                            switch (key)
                            {
                                case "Dialogue":
                                    var arguments = value.Split(',');
                                    var startTime = parseTimestamp(arguments[1]);
                                    var endTime = parseTimestamp(arguments[2]);
                                    var text = string.Join("\n", string.Join(",", arguments.Skip(9)).Split(new string[] { "\\N" }, StringSplitOptions.None));
                                    lines.Add(new Line(startTime, endTime, text));
                                    break;
                            }
                        });
                        break;
                }
            });
        return new Set(lines);
    }

    private static double parseTimestamp(string timestamp) => TimeSpan.Parse(timestamp).TotalMilliseconds;
}

// Note: moved the extension methods here to enbeed them into the parser 

internal static class StreamReaderExtensions
{
    public static void ParseSections(this StreamReader reader, Action<string> action)
    {
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            line = line.Trim();
            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                var sectionName = line[1..^1];
                action(sectionName);
            }
        }
    }

    public static void ParseSectionLines(this StreamReader reader, Action<string> action, bool trimLines = true)
    {
        var line = string.Empty;
        try
        {
            while ((line = reader.ReadLine()) != null)
            {
                if (trimLines) line = line.Trim();
                if (line.Length == 0) return;

                action(line);
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Failed to parse line \"{line}\".", e);
        }
    }

    public static void ParseKeyValueSection(this StreamReader reader, Action<string, string> action)
    {
        reader.ParseSectionLines(line =>
        {
            var separatorIndex = line.IndexOf(':');
            if (separatorIndex == -1) throw new InvalidDataException($"{line} is not a key/value");

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();

            action(key, value);
        });
    }
}