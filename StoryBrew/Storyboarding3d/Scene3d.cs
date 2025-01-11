using StoryBrew.Mapset;
using StoryBrew.Storyboarding;
using StoryBrew.Storyboarding.Commands;

namespace StoryBrew.Storyboarding3d;

public class Scene3d
{
    public required Node3d Root;

    public void Add(Object3d child)
    {
        Root.Add(child);
    }

    public void Generate(Camera camera, StoryboardSegment defaultSegment, double startTime, double endTime, double timeStep)
    {
        Root.GenerateTreeSprite(defaultSegment);
        for (var time = startTime; time < endTime + 5; time += timeStep)
            Root.GenerateTreeStates(time, camera);
        Root.GenerateTreeCommands();
    }

    public void Generate(Camera camera, StoryboardSegment defaultSegment, double startTime, double endTime, Beatmap beatmap, int divisor = 4)
    {
        Root.GenerateTreeSprite(defaultSegment);
        beatmap.ForEachTick((int)startTime, (int)endTime, divisor, (timingPoint, time, beatCount, tickCount) =>
            Root.GenerateTreeStates(time, camera));
        Root.GenerateTreeCommands();
    }

    public void Generate(Camera camera, StoryboardSegment defaultSegment, double startTime, double endTime, double timeStep, int loopCount, Action<LoopCommand, OsbSprite>? action = null)
    {
        Root.GenerateTreeSprite(defaultSegment);
        for (var time = startTime; time < endTime + 5; time += timeStep)
            Root.GenerateTreeStates(time, camera);
        Root.GenerateTreeLoopCommands(startTime, endTime, loopCount, action, offsetCommands: true);
    }
}
