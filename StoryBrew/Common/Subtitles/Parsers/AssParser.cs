﻿using StoryBrew.Util;
using System.Text;

namespace StoryBrew.Common.Subtitles.Parsers;

public static class AssParser
{
    public static SubtitleSet Parse(string path)
    {
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) return Parse(stream);
    }

    public static SubtitleSet Parse(Stream stream)
    {
        var lines = new List<SubtitleLine>();
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
                                    lines.Add(new SubtitleLine(startTime, endTime, text));
                                    break;
                            }
                        });
                        break;
                }
            });
        return new SubtitleSet(lines);
    }

    private static double parseTimestamp(string timestamp) => TimeSpan.Parse(timestamp).TotalMilliseconds;
}
