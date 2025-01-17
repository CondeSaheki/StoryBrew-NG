﻿using OpenTK.Mathematics;
using StoryBrew.Mapset;
using StoryBrew.Storyboarding.Commands;
using StoryBrew.Storyboarding.CommandValues;
using StoryBrew.Storyboarding.Display;
using StoryBrew.Util;
using StoryBrew.Project.Files;

namespace StoryBrew.Storyboarding;

public class Sprite : IStoryboardElement
{
    public static readonly Vector2 DEFAULT_POSITION = new(320, 240);

    private readonly List<ICommand> commands = [];
    private CommandGroup? currentCommandGroup = null;
    public bool InGroup => currentCommandGroup != null;

    /// <summary>
    /// If this sprite contains more than CommandSplitThreshold commands, they will be split between multiple sprites.
    /// Does not apply when the sprite has triggers.
    /// </summary>
    public int CommandSplitThreshold = 0;
    public string Path = string.Empty;

    public virtual string GetTexturePathAt(double time) => Path;

    public Origin Origin = Origin.Centre;

    private Vector2 initialPosition = DEFAULT_POSITION;
    public Vector2 InitialPosition
    {
        get => initialPosition;
        set
        {
            if (initialPosition == value) return;
            initialPosition = value;
            moveTimeline.DefaultValue = initialPosition;
            moveXTimeline.DefaultValue = initialPosition.X;
            moveYTimeline.DefaultValue = initialPosition.Y;
        }
    }

    public IEnumerable<ICommand> Commands => commands;

    public int CommandCount => commands.Count;

    public int CommandCost => commands.Sum(c => c.Cost);

    public bool HasIncompatibleCommands =>
        (moveTimeline.HasCommands && (moveXTimeline.HasCommands || moveYTimeline.HasCommands)) ||
        (scaleTimeline.HasCommands && scaleVecTimeline.HasCommands);

    public bool HasOverlappedCommands =>
        moveTimeline.HasOverlap ||
        moveXTimeline.HasOverlap ||
        moveYTimeline.HasOverlap ||
        scaleTimeline.HasOverlap ||
        scaleVecTimeline.HasOverlap ||
        rotateTimeline.HasOverlap ||
        fadeTimeline.HasOverlap ||
        colorTimeline.HasOverlap ||
        additiveTimeline.HasOverlap ||
        flipHTimeline.HasOverlap ||
        flipVTimeline.HasOverlap;

    public bool HasRotateCommands => rotateTimeline.HasCommands;
    public bool HasScalingCommands => scaleTimeline.HasCommands || scaleVecTimeline.HasCommands;
    public bool HasMoveXYCommands => moveXTimeline.HasCommands || moveYTimeline.HasCommands;

    private double commandsStartTime = double.MaxValue;
    public double CommandsStartTime
    {
        get
        {
            if (commandsStartTime == double.MaxValue)
                refreshStartEndTimes();
            return commandsStartTime;
        }
    }

    private double commandsEndTime = double.MinValue;
    public double CommandsEndTime
    {
        get
        {
            if (commandsEndTime == double.MinValue)
                refreshStartEndTimes();
            return commandsEndTime;
        }
    }

    private void refreshStartEndTimes()
    {
        clearStartEndTimes();
        foreach (var command in commands)
        {
            if (!command.Active) continue;
            commandsStartTime = Math.Min(commandsStartTime, command.StartTime);
            commandsEndTime = Math.Max(commandsEndTime, command.EndTime);
        }
    }

    private void clearStartEndTimes()
    {
        commandsStartTime = double.MaxValue;
        commandsEndTime = double.MinValue;
    }

    public Sprite()
    {
        initializeDisplayValueBuilders();
    }

    public Sprite(string path, Origin origin, Vector2 initialPosition ) : this()
    {
        Path = path;
        Origin = origin;
        InitialPosition = initialPosition;
    }

    public Sprite(string path, Origin origin = Origin.Centre) : this(path, origin, DEFAULT_POSITION) { }

    public void Move(Easing easing, double startTime, double endTime, CommandPosition startPosition, CommandPosition endPosition) => addCommand(new MoveCommand(easing, startTime, endTime, startPosition, endPosition));
    public void Move(Easing easing, double startTime, double endTime, CommandPosition startPosition, double endX, double endY) => Move(easing, startTime, endTime, startPosition, new CommandPosition(endX, endY));
    public void Move(Easing easing, double startTime, double endTime, double startX, double startY, double endX, double endY) => Move(easing, startTime, endTime, new CommandPosition(startX, startY), new CommandPosition(endX, endY));
    public void Move(double startTime, double endTime, CommandPosition startPosition, CommandPosition endPosition) => Move(Easing.None, startTime, endTime, startPosition, endPosition);
    public void Move(double startTime, double endTime, CommandPosition startPosition, double endX, double endY) => Move(Easing.None, startTime, endTime, startPosition, endX, endY);
    public void Move(double startTime, double endTime, double startX, double startY, double endX, double endY) => Move(Easing.None, startTime, endTime, startX, startY, endX, endY);
    public void Move(double time, CommandPosition position) => Move(Easing.None, time, time, position, position);
    public void Move(double time, double x, double y) => Move(Easing.None, time, time, x, y, x, y);

    public void MoveX(Easing easing, double startTime, double endTime, CommandDecimal startX, CommandDecimal endX) => addCommand(new MoveXCommand(easing, startTime, endTime, startX, endX));
    public void MoveX(double startTime, double endTime, CommandDecimal startX, CommandDecimal endX) => MoveX(Easing.None, startTime, endTime, startX, endX);
    public void MoveX(double time, CommandDecimal x) => MoveX(Easing.None, time, time, x, x);

    public void MoveY(Easing easing, double startTime, double endTime, CommandDecimal startY, CommandDecimal endY) => addCommand(new MoveYCommand(easing, startTime, endTime, startY, endY));
    public void MoveY(double startTime, double endTime, CommandDecimal startY, CommandDecimal endY) => MoveY(Easing.None, startTime, endTime, startY, endY);
    public void MoveY(double time, CommandDecimal y) => MoveY(Easing.None, time, time, y, y);

    public void Scale(Easing easing, double startTime, double endTime, CommandDecimal startScale, CommandDecimal endScale) => addCommand(new ScaleCommand(easing, startTime, endTime, startScale, endScale));
    public void Scale(double startTime, double endTime, CommandDecimal startScale, CommandDecimal endScale) => Scale(Easing.None, startTime, endTime, startScale, endScale);
    public void Scale(double time, CommandDecimal scale) => Scale(Easing.None, time, time, scale, scale);

    public void ScaleVec(Easing easing, double startTime, double endTime, CommandScale startScale, CommandScale endScale) => addCommand(new VScaleCommand(easing, startTime, endTime, startScale, endScale));
    public void ScaleVec(Easing easing, double startTime, double endTime, CommandScale startScale, double endX, double endY) => ScaleVec(easing, startTime, endTime, startScale, new CommandScale(endX, endY));
    public void ScaleVec(Easing easing, double startTime, double endTime, double startX, double startY, double endX, double endY) => ScaleVec(easing, startTime, endTime, new CommandScale(startX, startY), new CommandScale(endX, endY));
    public void ScaleVec(double startTime, double endTime, CommandScale startScale, CommandScale endScale) => ScaleVec(Easing.None, startTime, endTime, startScale, endScale);
    public void ScaleVec(double startTime, double endTime, CommandScale startScale, double endX, double endY) => ScaleVec(Easing.None, startTime, endTime, startScale, endX, endY);
    public void ScaleVec(double startTime, double endTime, double startX, double startY, double endX, double endY) => ScaleVec(Easing.None, startTime, endTime, startX, startY, endX, endY);
    public void ScaleVec(double time, CommandScale scale) => ScaleVec(Easing.None, time, time, scale, scale);
    public void ScaleVec(double time, double x, double y) => ScaleVec(Easing.None, time, time, x, y, x, y);

    public void Rotate(Easing easing, double startTime, double endTime, CommandDecimal startRotation, CommandDecimal endRotation) => addCommand(new RotateCommand(easing, startTime, endTime, startRotation, endRotation));
    public void Rotate(double startTime, double endTime, CommandDecimal startRotation, CommandDecimal endRotation) => Rotate(Easing.None, startTime, endTime, startRotation, endRotation);
    public void Rotate(double time, CommandDecimal rotation) => Rotate(Easing.None, time, time, rotation, rotation);

    public void Fade(Easing easing, double startTime, double endTime, CommandDecimal startOpacity, CommandDecimal endOpacity) => addCommand(new FadeCommand(easing, startTime, endTime, startOpacity, endOpacity));
    public void Fade(double startTime, double endTime, CommandDecimal startOpacity, CommandDecimal endOpacity) => Fade(Easing.None, startTime, endTime, startOpacity, endOpacity);
    public void Fade(double time, CommandDecimal opacity) => Fade(Easing.None, time, time, opacity, opacity);

    public void Color(Easing easing, double startTime, double endTime, CommandColor startColor, CommandColor endColor) => addCommand(new ColorCommand(easing, startTime, endTime, startColor, endColor));
    public void Color(Easing easing, double startTime, double endTime, CommandColor startColor, double endRed, double endGreen, double endBlue) => Color(easing, startTime, endTime, startColor, new CommandColor(endRed, endGreen, endBlue));
    public void Color(Easing easing, double startTime, double endTime, double startRed, double startGreen, double startBlue, double endRed, double endGreen, double endBlue) => Color(easing, startTime, endTime, new CommandColor(startRed, startGreen, startBlue), new CommandColor(endRed, endGreen, endBlue));
    public void Color(double startTime, double endTime, CommandColor startColor, CommandColor endColor) => Color(Easing.None, startTime, endTime, startColor, endColor);
    public void Color(double startTime, double endTime, CommandColor startColor, double endRed, double endGreen, double endBlue) => Color(Easing.None, startTime, endTime, startColor, endRed, endGreen, endBlue);
    public void Color(double startTime, double endTime, double startRed, double startGreen, double startBlue, double endRed, double endGreen, double endBlue) => Color(Easing.None, startTime, endTime, startRed, startGreen, startBlue, endRed, endGreen, endBlue);
    public void Color(double time, CommandColor color) => Color(Easing.None, time, time, color, color);
    public void Color(double time, double red, double green, double blue) => Color(Easing.None, time, time, red, green, blue, red, green, blue);

    public void ColorHsb(Easing easing, double startTime, double endTime, CommandColor startColor, double endHue, double endSaturation, double endBrightness) => Color(easing, startTime, endTime, startColor, CommandColor.FromHsb(endHue, endSaturation, endBrightness));
    public void ColorHsb(Easing easing, double startTime, double endTime, double startHue, double startSaturation, double startBrightness, double endHue, double endSaturation, double endBrightness) => Color(easing, startTime, endTime, CommandColor.FromHsb(startHue, startSaturation, startBrightness), CommandColor.FromHsb(endHue, endSaturation, endBrightness));
    public void ColorHsb(double startTime, double endTime, CommandColor startColor, double endHue, double endSaturation, double endBrightness) => ColorHsb(Easing.None, startTime, endTime, startColor, endHue, endSaturation, endBrightness);
    public void ColorHsb(double startTime, double endTime, double startHue, double startSaturation, double startBrightness, double endHue, double endSaturation, double endBrightness) => ColorHsb(Easing.None, startTime, endTime, startHue, startSaturation, startBrightness, endHue, endSaturation, endBrightness);
    public void ColorHsb(double time, double hue, double saturation, double brightness) => ColorHsb(Easing.None, time, time, hue, saturation, brightness, hue, saturation, brightness);

    public void Parameter(Easing easing, double startTime, double endTime, CommandParameter parameter) => addCommand(new ParameterCommand(easing, startTime, endTime, parameter));
    public void FlipH(double startTime, double endTime) => Parameter(Easing.None, startTime, endTime, CommandParameter.FLIP_HORIZONTAL);
    public void FlipH(double time) => FlipH(time, time);
    public void FlipV(double startTime, double endTime) => Parameter(Easing.None, startTime, endTime, CommandParameter.FLIP_VERTICAL);
    public void FlipV(double time) => FlipV(time, time);
    public void Additive(double startTime, double endTime) => Parameter(Easing.None, startTime, endTime, CommandParameter.ADDITIVE_BLENDING);
    public void Additive(double time) => Additive(time, time);

    public LoopCommand StartLoopGroup(double startTime, int loopCount)
    {
        var loopCommand = new LoopCommand(startTime, loopCount);
        addCommand(loopCommand);
        startDisplayLoop(loopCommand);
        return loopCommand;
    }

    public TriggerCommand StartTriggerGroup(string triggerName, double startTime, double endTime, int group = 0)
    {
        var triggerCommand = new TriggerCommand(triggerName, startTime, endTime, group);
        addCommand(triggerCommand);
        startDisplayTrigger(triggerCommand);
        return triggerCommand;
    }

    public void EndGroup()
    {
        currentCommandGroup?.EndGroup();
        currentCommandGroup = null;

        endDisplayComposites();
    }

    private void addCommand(ICommand command)
    {
        if (command is CommandGroup commandGroup)
        {
            currentCommandGroup = commandGroup;
            commands.Add(commandGroup);
        }
        else
        {
            if (currentCommandGroup != null)
                currentCommandGroup.Add(command);
            else
                commands.Add(command);
            addDisplayCommand(command);
        }
        clearStartEndTimes();
    }

    public void AddCommand(ICommand command)
    {
        if (command is ColorCommand colorCommand) Color(colorCommand.Easing, colorCommand.StartTime, colorCommand.EndTime, colorCommand.StartValue, colorCommand.EndValue);
        else if (command is FadeCommand fadeCommand) Fade(fadeCommand.Easing, fadeCommand.StartTime, fadeCommand.EndTime, fadeCommand.StartValue, fadeCommand.EndValue);
        else if (command is ScaleCommand scaleCommand) Scale(scaleCommand.Easing, scaleCommand.StartTime, scaleCommand.EndTime, scaleCommand.StartValue, scaleCommand.EndValue);
        else if (command is VScaleCommand vScaleCommand) ScaleVec(vScaleCommand.Easing, vScaleCommand.StartTime, vScaleCommand.EndTime, vScaleCommand.StartValue, vScaleCommand.EndValue);
        else if (command is ParameterCommand parameterCommand) Parameter(parameterCommand.Easing, parameterCommand.StartTime, parameterCommand.EndTime, parameterCommand.StartValue);
        else if (command is MoveCommand moveCommand) Move(moveCommand.Easing, moveCommand.StartTime, moveCommand.EndTime, moveCommand.StartValue, moveCommand.EndValue);
        else if (command is MoveXCommand moveXCommand) MoveX(moveXCommand.Easing, moveXCommand.StartTime, moveXCommand.EndTime, moveXCommand.StartValue, moveXCommand.EndValue);
        else if (command is MoveYCommand moveYCommand) MoveY(moveYCommand.Easing, moveYCommand.StartTime, moveYCommand.EndTime, moveYCommand.StartValue, moveYCommand.EndValue);
        else if (command is RotateCommand rotateCommand) Rotate(rotateCommand.Easing, rotateCommand.StartTime, rotateCommand.EndTime, rotateCommand.StartValue, rotateCommand.EndValue);
        else if (command is LoopCommand loopCommand)
        {
            StartLoopGroup(loopCommand.StartTime, loopCommand.LoopCount);
            foreach (var cmd in loopCommand.Commands)
                AddCommand(cmd);
            EndGroup();
        }
        else if (command is TriggerCommand triggerCommand)
        {
            StartTriggerGroup(triggerCommand.TriggerName, triggerCommand.StartTime, triggerCommand.EndTime, triggerCommand.Group);
            foreach (var cmd in triggerCommand.Commands)
                AddCommand(cmd);
            EndGroup();
        }
        else throw new NotSupportedException($"Failed to add command: No support for adding command of type {command.GetType().FullName}");
    }

    #region Display

    private readonly List<KeyValuePair<Predicate<ICommand>, IAnimatedValueBuilder>> displayValueBuilders = new List<KeyValuePair<Predicate<ICommand>, IAnimatedValueBuilder>>();

    private readonly AnimatedValue<CommandPosition> moveTimeline = new(Vector2.Zero);
    private readonly AnimatedValue<CommandDecimal> moveXTimeline = new(0);
    private readonly AnimatedValue<CommandDecimal> moveYTimeline = new(0);
    private readonly AnimatedValue<CommandDecimal> scaleTimeline = new(1);
    private readonly AnimatedValue<CommandScale> scaleVecTimeline = new(Vector2.One);
    private readonly AnimatedValue<CommandDecimal> rotateTimeline = new(0);
    private readonly AnimatedValue<CommandDecimal> fadeTimeline = new(1);
    private readonly AnimatedValue<CommandColor> colorTimeline = new(CommandColor.FromRgb(255, 255, 255));
    private readonly AnimatedValue<CommandParameter> additiveTimeline = new(CommandParameter.NONE);
    private readonly AnimatedValue<CommandParameter> flipHTimeline = new(CommandParameter.NONE);
    private readonly AnimatedValue<CommandParameter> flipVTimeline = new(CommandParameter.NONE);

    public CommandPosition PositionAt(double time) => moveTimeline.HasCommands ? moveTimeline.ValueAtTime(time) : new(moveXTimeline.ValueAtTime(time), moveYTimeline.ValueAtTime(time));
    public CommandScale ScaleAt(double time) => scaleVecTimeline.HasCommands ? scaleVecTimeline.ValueAtTime(time) : new(scaleTimeline.ValueAtTime(time));
    public CommandDecimal RotationAt(double time) => rotateTimeline.ValueAtTime(time);
    public CommandDecimal OpacityAt(double time) => fadeTimeline.ValueAtTime(time);
    public CommandColor ColorAt(double time) => colorTimeline.ValueAtTime(time);
    public CommandParameter AdditiveAt(double time) => additiveTimeline.ValueAtTime(time);
    public CommandParameter FlipHAt(double time) => flipHTimeline.ValueAtTime(time);
    public CommandParameter FlipVAt(double time) => flipVTimeline.ValueAtTime(time);

    private void initializeDisplayValueBuilders()
    {
        displayValueBuilders.Add(new((c) => c is MoveCommand, new AnimatedValueBuilder<CommandPosition>(moveTimeline)));
        displayValueBuilders.Add(new((c) => c is MoveXCommand, new AnimatedValueBuilder<CommandDecimal>(moveXTimeline)));
        displayValueBuilders.Add(new((c) => c is MoveYCommand, new AnimatedValueBuilder<CommandDecimal>(moveYTimeline)));
        displayValueBuilders.Add(new((c) => c is ScaleCommand, new AnimatedValueBuilder<CommandDecimal>(scaleTimeline)));
        displayValueBuilders.Add(new((c) => c is VScaleCommand, new AnimatedValueBuilder<CommandScale>(scaleVecTimeline)));
        displayValueBuilders.Add(new((c) => c is RotateCommand, new AnimatedValueBuilder<CommandDecimal>(rotateTimeline)));
        displayValueBuilders.Add(new((c) => c is FadeCommand, new AnimatedValueBuilder<CommandDecimal>(fadeTimeline)));
        displayValueBuilders.Add(new((c) => c is ColorCommand, new AnimatedValueBuilder<CommandColor>(colorTimeline)));
        displayValueBuilders.Add(new((c) => c is ParameterCommand { StartValue.Type: ParameterType.AdditiveBlending }, new AnimatedValueBuilder<CommandParameter>(additiveTimeline)));
        displayValueBuilders.Add(new((c) => c is ParameterCommand { StartValue.Type: ParameterType.FlipHorizontal }, new AnimatedValueBuilder<CommandParameter>(flipHTimeline)));
        displayValueBuilders.Add(new((c) => c is ParameterCommand { StartValue.Type: ParameterType.FlipVertical }, new AnimatedValueBuilder<CommandParameter>(flipVTimeline)));
    }

    private void addDisplayCommand(ICommand command)
    {
        foreach (var builders in displayValueBuilders)
            if (builders.Key(command))
                builders.Value.Add(command);
    }

    private void startDisplayLoop(LoopCommand loopCommand)
    {
        foreach (var builders in displayValueBuilders)
            builders.Value.StartDisplayLoop(loopCommand);
    }

    private void startDisplayTrigger(TriggerCommand triggerCommand)
    {
        foreach (var builders in displayValueBuilders)
            builders.Value.StartDisplayTrigger(triggerCommand);
    }

    private void endDisplayComposites()
    {
        foreach (var builders in displayValueBuilders)
            builders.Value.EndDisplayComposite();
    }

    #endregion

    public bool IsActive(double time)
        => CommandsStartTime <= time && time <= CommandsEndTime;

    public double StartTime => CommandsStartTime;
    public double EndTime => CommandsEndTime;

    public void WriteOsb(TextWriter writer, ExportSettings exportSettings, Layer layer, StoryboardTransform? transform)
    {
        if (CommandCount == 0)
            return;

        var osbSpriteWriter = OsbWriterFactory.CreateWriter(this, moveTimeline,
                                                                  moveXTimeline,
                                                                  moveYTimeline,
                                                                  scaleTimeline,
                                                                  scaleVecTimeline,
                                                                  rotateTimeline,
                                                                  fadeTimeline,
                                                                  colorTimeline,
                                                                  writer, exportSettings, layer);
        osbSpriteWriter.WriteOsb(transform ?? throw new ArgumentNullException(nameof(transform)));
    }

    public static bool InScreenBounds(Vector2 position, Vector2 size, float rotation, Vector2 origin)
        => new OrientedBoundingBox(position, origin, size.X, size.Y, rotation).Intersects(OsuHitObject.WIDESCREEN_STORYBOARD_BOUNDS);

    public static Vector2 GetOriginVector(Origin origin, float width, float height)
    {
        switch (origin)
        {
            case Origin.TopLeft: return Vector2.Zero;
            case Origin.TopCentre: return new Vector2(width * 0.5f, 0);
            case Origin.TopRight: return new Vector2(width, 0);
            case Origin.CentreLeft: return new Vector2(0, height * 0.5f);
            case Origin.Centre: return new Vector2(width * 0.5f, height * 0.5f);
            case Origin.CentreRight: return new Vector2(width, height * 0.5f);
            case Origin.BottomLeft: return new Vector2(0, height);
            case Origin.BottomCentre: return new Vector2(width * 0.5f, height);
            case Origin.BottomRight: return new Vector2(width, height);
        }
        throw new NotSupportedException(origin.ToString());
    }
}
