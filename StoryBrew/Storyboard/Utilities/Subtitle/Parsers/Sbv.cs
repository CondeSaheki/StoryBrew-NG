// Note: YouTube's subtitle format

using System.Text;

namespace StoryBrew.Storyboard.Utilities.Subtitle.Parsers;

public static class Sbv
{
    public static Set Parse(string path)
    {
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) return Parse(stream);
    }

    public static Set Parse(Stream stream)
    {
        var lines = new List<Line>();
        foreach (var block in parseBlocks(stream))
        {
            var blockLines = block.Split('\n');
            var timestamps = blockLines[0].Split(',');
            var startTime = parseTimestamp(timestamps[0]);
            var endTime = parseTimestamp(timestamps[1]);
            var text = string.Join("\n", blockLines, 1, blockLines.Length - 1);
            lines.Add(new Line(startTime, endTime, text));
        }
        return new Set(lines);
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
