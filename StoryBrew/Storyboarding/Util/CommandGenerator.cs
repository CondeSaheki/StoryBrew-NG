using OpenTK.Mathematics;
using StoryBrew.Animations;
using StoryBrew.Mapset;
using StoryBrew.Scripting;
using StoryBrew.Storyboarding.CommandValues;
using StoryBrew.Util;

namespace StoryBrew.Storyboarding.Util;

public class CommandGenerator
{
    private readonly List<State> states = [];

    private readonly KeyframedValue<Vector2> positions = new(InterpolatingFunctions.Vector2, Vector2.Zero);
    private readonly KeyframedValue<Vector2> scales = new(InterpolatingFunctions.Vector2, Vector2.Zero);
    private readonly KeyframedValue<float> rotations = new(InterpolatingFunctions.FloatAngle, 0);
    private readonly KeyframedValue<CommandColor> colors = new(InterpolatingFunctions.CommandColor, new CommandColor(0,0,0));
    private readonly KeyframedValue<float> opacities = new(InterpolatingFunctions.Float, 0);

    private readonly KeyframedValue<Vector2> finalPositions = new(InterpolatingFunctions.Vector2, Vector2.Zero);
    private readonly KeyframedValue<Vector2> finalScales = new(InterpolatingFunctions.Vector2, Vector2.Zero);
    private readonly KeyframedValue<float> finalRotations = new(InterpolatingFunctions.FloatAngle, 0);
    private readonly KeyframedValue<CommandColor> finalColors = new(InterpolatingFunctions.CommandColor, new CommandColor(0,0,0));
    private readonly KeyframedValue<float> finalOpacities = new(InterpolatingFunctions.Float, 0);

    private readonly KeyframedValue<bool> flipH = new(InterpolatingFunctions.BoolFrom, false);
    private readonly KeyframedValue<bool> flipV = new(InterpolatingFunctions.BoolFrom, false);
    private readonly KeyframedValue<bool> additive = new(InterpolatingFunctions.BoolFrom, false);

    public State? StartState => states.Count == 0 ? null : states[0];
    public State? EndState => states.Count == 0 ? null : states[^1];

    public double PositionTolerance = 1;
    public double ScaleTolerance = 0.1;
    public double RotationTolerance = 0.001;
    public double ColorTolerance = 2;
    public double OpacityTolerance = 0.01;

    public int PositionDecimals = 1;
    public int ScaleDecimals = 2;
    public int RotationDecimals = 3;
    public int OpacityDecimals = 2;

    public Action<State>? PostProcess;

    public void Add(State state, bool before = false)
    {
        if (states.Count == 0 || states[^1].Time < state.Time)
            states.Add(state);
        else
        {
            var index = states.BinarySearch(state);
            if (index >= 0)
            {
                if (before)
                    while (index > 0 && states[index].Time >= state.Time) index--;
                else while (index < states.Count && states[index].Time <= state.Time) index++;
            }
            else index = ~index;
            states.Insert(index, state);
        }
    }

    public void ClearStates()
        => states.Clear();

    public bool GenerateCommands(Sprite sprite, Action<Action, Sprite>? action = null, double? startTime = null, double? endTime = null, double timeOffset = 0, bool loopable = false)
        => GenerateCommands(sprite, OsuHitObject.WIDESCREEN_STORYBOARD_BOUNDS, action, startTime, endTime, timeOffset, loopable);

    public bool GenerateCommands(Sprite sprite, Box2 bounds, Action<Action, Sprite>? action = null, double? startTime = null, double? endTime = null, double timeOffset = 0, bool loopable = false)
    {
        throw new NotImplementedException();
        /*

        State? previousState = null;
        var wasVisible = false;
        var everVisible = false;
        var stateAdded = false;
        var imageSize = Vector2.One;

        foreach (var state in states)
        {
            var time = state.Time + timeOffset;
            var bitmap = StoryboardObjectGenerator.Current?.GetMapsetBitmap(sprite.GetTexturePathAt(time)) ?? throw new Exception();
            imageSize = new Vector2(bitmap.Width, bitmap.Height);

            PostProcess?.Invoke(state);
            var isVisible = state.IsVisible(bitmap.Width, bitmap.Height, sprite.Origin, bounds);

            if (isVisible) everVisible = true;
            if (!wasVisible && isVisible)
            {
                if (!stateAdded && previousState != null)
                    addKeyframes(previousState, time);
                addKeyframes(state, time);
                stateAdded = true;
            }
            else if (wasVisible && !isVisible)
            {
                addKeyframes(state, time);
                commitKeyframes(imageSize);
                stateAdded = true;
            }
            else if (isVisible)
            {
                addKeyframes(state, time);
                stateAdded = true;
            }
            else stateAdded = false;

            previousState = state;
            wasVisible = isVisible;
        }

        if (wasVisible)
            commitKeyframes(imageSize);

        if (everVisible)
        {
            if (action != null)
                action(() => convertToCommands(sprite, startTime, endTime, timeOffset, loopable), sprite);
            else convertToCommands(sprite, startTime, endTime, timeOffset, loopable);
        }

        clearFinalKeyframes();
        return everVisible;
        */
    }

    private void commitKeyframes(Vector2 imageSize)
    {
        positions.Simplify2dKeyframes(PositionTolerance, p => p);
        positions.TransferKeyframes(finalPositions);

        scales.Simplify2dKeyframes(ScaleTolerance, s => new Vector2(s.X * imageSize.X, s.Y * imageSize.Y));
        scales.TransferKeyframes(finalScales);

        rotations.Simplify1dKeyframes(RotationTolerance, a => a);
        rotations.TransferKeyframes(finalRotations);

        colors.Simplify3dKeyframes(ColorTolerance, c => new Vector3(c.R, c.G, c.B));
        colors.TransferKeyframes(finalColors);

        opacities.Simplify1dKeyframes(OpacityTolerance, o => o);
        if (opacities.StartValue > 0) opacities.Add(opacities.StartTime, 0, before: true);
        if (opacities.EndValue > 0) opacities.Add(opacities.EndTime, 0);
        opacities.TransferKeyframes(finalOpacities);
    }

    private void convertToCommands(Sprite sprite, double? startTime, double? endTime, double timeOffset, bool loopable)
    {
        var startStateTime = loopable ? (startTime ?? StartState?.Time) + timeOffset : null;
        var endStateTime = loopable ? (endTime ?? EndState?.Time) + timeOffset : null;

        finalPositions.ForEachPair((start, end) => sprite.Move(start.Time, end.Time, start.Value, end.Value), new Vector2(320, 240),
            p => new Vector2((float)Math.Round(p.X, PositionDecimals), (float)Math.Round(p.Y, PositionDecimals)), startStateTime, loopable: loopable);
        var useVectorScaling = finalScales.Any(k => k.Value.X != k.Value.Y);
        finalScales.ForEachPair((start, end) =>
        {
            if (useVectorScaling)
                sprite.ScaleVec(start.Time, end.Time, start.Value, end.Value);
            else sprite.Scale(start.Time, end.Time, start.Value.X, end.Value.X);
        }, Vector2.One, s => new Vector2((float)Math.Round(s.X, ScaleDecimals), (float)Math.Round(s.Y, ScaleDecimals)), startStateTime, loopable: loopable);
        finalRotations.ForEachPair((start, end) => sprite.Rotate(start.Time, end.Time, start.Value, end.Value), 0,
            r => (float)Math.Round(r, RotationDecimals), startStateTime, loopable: loopable);
        finalColors.ForEachPair((start, end) => sprite.Color(start.Time, end.Time, start.Value, end.Value), CommandColor.WHITE,
            c => CommandColor.FromRgb(c.R, c.G, c.B), startStateTime, loopable: loopable);
        finalOpacities.ForEachPair((start, end) => sprite.Fade(start.Time, end.Time, start.Value, end.Value), -1,
            o => (float)Math.Round(o, OpacityDecimals), startStateTime, endStateTime, loopable: loopable);

        flipH.ForEachFlag(sprite.FlipH);
        flipV.ForEachFlag(sprite.FlipV);
        additive.ForEachFlag(sprite.Additive);
    }

    private void addKeyframes(State state, double time)
    {
        positions.Add(time, state.Position);
        scales.Add(time, state.Scale);
        rotations.Add(time, (float)state.Rotation);
        colors.Add(time, state.Color);
        opacities.Add(time, (float)state.Opacity);
        flipH.Add(time, state.FlipH);
        flipV.Add(time, state.FlipV);
        additive.Add(time, state.Additive);
    }

    private void clearFinalKeyframes()
    {
        finalPositions.Clear();
        finalScales.Clear();
        finalRotations.Clear();
        finalColors.Clear();
        finalOpacities.Clear();
        flipH.Clear();
        flipV.Clear();
        additive.Clear();
    }

    public class State : IComparable<State>
    {
        public double Time;
        public Vector2 Position = new Vector2(320, 240);
        public Vector2 Scale = new Vector2(1, 1);
        public double Rotation = 0;
        public CommandColor Color = CommandColor.WHITE;
        public double Opacity = 1;
        public bool FlipH;
        public bool FlipV;
        public bool Additive;

        public bool IsVisible(int width, int height, Origin origin, Box2 bounds)
        {
            if (Opacity <= 0)
                return false;

            if (Scale.X == 0 || Scale.Y == 0)
                return false;

            if (Additive && Color.R == 0 && Color.G == 0 && Color.B == 0)
                return false;

            if (!bounds.ContainsInclusive(Position))
            {
                var w = width * Scale.X;
                var h = height * Scale.Y;
                Vector2 originVector;
                switch (origin)
                {
                    default:
                    case Origin.TopLeft: originVector = Vector2.Zero; break;
                    case Origin.TopCentre: originVector = new Vector2(w * 0.5f, 0); break;
                    case Origin.TopRight: originVector = new Vector2(w, 0); break;
                    case Origin.CentreLeft: originVector = new Vector2(0, h * 0.5f); break;
                    case Origin.Centre: originVector = new Vector2(w * 0.5f, h * 0.5f); break;
                    case Origin.CentreRight: originVector = new Vector2(w, h * 0.5f); break;
                    case Origin.BottomLeft: originVector = new Vector2(0, h); break;
                    case Origin.BottomCentre: originVector = new Vector2(w * 0.5f, h); break;
                    case Origin.BottomRight: originVector = new Vector2(w, h); break;
                }
                var obb = new OrientedBoundingBox(Position, originVector, w, h, Rotation);
                if (!obb.Intersects(bounds))
                    return false;
            }

            return true;
        }

        public int CompareTo(State? other)
            => Math.Sign(Time - other?.Time ?? 0);
    }
}
