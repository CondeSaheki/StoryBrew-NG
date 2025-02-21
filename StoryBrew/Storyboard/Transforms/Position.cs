using System.Globalization;
using System.Text;
using OpenTK.Mathematics;
using StoryBrew.Storyboard.Common;
using StoryBrew.Storyboard.Core;

namespace StoryBrew.Storyboard.Transforms;

public class Position : Transform<Vector2>
{
    private Axis affect = Axis.Both;

    public Position(Ease ease, double startTime, double endTime, Vector2 startValue, Vector2 endValue)
        : base(ease, startTime, endTime, startValue, endValue)
    {
    }

    public Position(Ease ease, double startTime, double endTime, float startXValue, float startYValue, float endXValue, float endYValue)
        : this(ease, startTime, endTime, new(startXValue, startYValue), new(endXValue, endYValue))
    {
    }

    protected Position(Ease ease, double startTime, double endTime, float startXValue, float startYValue, float endXValue, float endYValue, Axis affect)
        : this(ease, startTime, endTime, new(startXValue, startYValue), new(endXValue, endYValue))
    {
        this.affect = affect;
    }

    // (Easing.Type easing, double startTime, double endTime, CommandPosition startPosition, CommandPosition endPosition) => addCommand(new MoveCommand(easing, startTime, endTime, startPosition, endPosition));
    // (Easing.Type easing, double startTime, double endTime, CommandPosition startPosition, double endX, double endY) => Move(easing, startTime, endTime, startPosition, new CommandPosition(endX, endY));
    // (Easing.Type easing, double startTime, double endTime, double startX, double startY, double endX, double endY) => Move(easing, startTime, endTime, new CommandPosition(startX, startY), new CommandPosition(endX, endY));
    // (double startTime, double endTime, CommandPosition startPosition, CommandPosition endPosition) => Move(Easing.Type.None, startTime, endTime, startPosition, endPosition);
    // (double startTime, double endTime, CommandPosition startPosition, double endX, double endY) => Move(Easing.Type.None, startTime, endTime, startPosition, endX, endY);
    // (double startTime, double endTime, double startX, double startY, double endX, double endY) => Move(Easing.Type.None, startTime, endTime, startX, startY, endX, endY);
    // (double time, CommandPosition position) => Move(Easing.Type.None, time, time, position, position);
    // (double time, double x, double y) => Move(Easing.Type.None, time, time, x, y, x, y);

    public override string ToString()
    {
        switch (affect)
        {
            case Axis.Both: return $"Position -> {StartTime} -> {EndTime}, {StartValue.X} -> {EndValue.X}, {StartValue.Y} -> {EndValue.Y} {Ease}";
            case Axis.X: return $"PositionX -> {StartTime} -> {EndTime}, {StartValue.X} -> {EndValue.X} {Ease}";
            case Axis.Y: return $"PositionY -> {StartTime} -> {EndTime}, {StartValue.Y} -> {EndValue.Y} {Ease}";

            default: throw new NotImplementedException();
        }
    }

    internal override void Write(StringBuilder writer, Layer layer, uint depth = 0)
    {
        // Note: Aditionaly convert M to MX or MY when 

        const bool float_time = false; // Note: This will be implemented as param in next update

        var indentation = new string(' ', (int)depth);

        string identifier = affect switch
        {
            Axis.Both => "M",
            Axis.X => "MX",
            Axis.Y => "MY",

            _ => throw new NotImplementedException()
        };

        var easing = ((int)Ease).ToString();
        var startTime = (float_time ? StartTime : (int)StartTime).ToString(CultureInfo.InvariantCulture);
        var endTime = (float_time ? EndTime : (int)EndTime).ToString(CultureInfo.InvariantCulture);
        var startXValue = ((int)StartValue.X).ToString(CultureInfo.InvariantCulture);
        var endXValue = ((int)EndValue.X).ToString(CultureInfo.InvariantCulture);
        var startYValue = ((int)StartValue.Y).ToString(CultureInfo.InvariantCulture);
        var endYValue = ((int)EndValue.Y).ToString(CultureInfo.InvariantCulture);

        string resultM = $"{indentation}{identifier},{easing},{startTime},{endTime},{startXValue},{startYValue},{endXValue},{endYValue}";
        // string resultMx = $"{indentation}{identifier},{easing},{startTime},{endTime},{startXValue},{endXValue}";
        // string resultMy = $"{indentation}{identifier},{easing},{startTime},{endTime},{startYValue},{endYValue}";

        writer.AppendLine(resultM);
    }

    protected enum Axis
    {
        Both,
        X,
        Y
    }
}
