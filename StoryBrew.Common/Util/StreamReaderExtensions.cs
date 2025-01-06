namespace StoryBrew.Common.Util;

internal static class StreamReaderExtensions
{
    public static void ParseSections(this StreamReader reader, Action<string> action)
    {
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            line = line.Trim();
            if (line.StartsWith("[") && line.EndsWith("]"))
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
            var separatorIndex = line.IndexOf(":");
            if (separatorIndex == -1) throw new InvalidDataException($"{line} is not a key/value");

            var key = line[..separatorIndex].Trim();
            var value = line.Substring(separatorIndex + 1, line.Length - 1 - separatorIndex).Trim();

            action(key, value);
        });
    }
}
