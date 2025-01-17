﻿using System.Text;

namespace StoryBrew.Common.Subtitles.Parsers;

// YouTube's subtitle format
public static class SbvParser
{
    public static SubtitleSet Parse(string path)
    {
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) return Parse(stream);
    }

    public static SubtitleSet Parse(Stream stream)
    {
        var lines = new List<SubtitleLine>();
        foreach (var block in parseBlocks(stream))
        {
            var blockLines = block.Split('\n');
            var timestamps = blockLines[0].Split(',');
            var startTime = parseTimestamp(timestamps[0]);
            var endTime = parseTimestamp(timestamps[1]);
            var text = string.Join("\n", blockLines, 1, blockLines.Length - 1);
            lines.Add(new SubtitleLine(startTime, endTime, text));
        }
        return new SubtitleSet(lines);
    }

    private static IEnumerable<string> parseBlocks(Stream stream)
    {
        using (var reader = new StreamReader(stream))
        {
            var sb = new StringBuilder();

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line.Trim()))
                {
                    var block = sb.ToString().Trim();
                    if (block.Length > 0) yield return block;
                    sb.Clear();
                }
                else sb.AppendLine(line);
            }

            var endBlock = sb.ToString().Trim();
            if (endBlock.Length > 0) yield return endBlock;
        }
    }

    private static double parseTimestamp(string timestamp) => TimeSpan.Parse(timestamp).TotalMilliseconds;
}
