namespace StoryBrew.Storyboarding.Commands;

public class CommandComparer : Comparer<ICommand>
{
    public override int Compare(ICommand? x, ICommand? y) => CompareCommands(x, y);

    public static int CompareCommands(ICommand? x, ICommand? y)
    {
        if (x == null && y == null) return 0;
        var result = ((int)Math.Round(x?.StartTime ?? 0)).CompareTo((int)Math.Round(y?.StartTime ?? 0));
        if (result != 0) return result;

        return ((int)Math.Round(x?.EndTime ?? 0)).CompareTo((int)Math.Round(y?.EndTime ?? 0));
    }
}
