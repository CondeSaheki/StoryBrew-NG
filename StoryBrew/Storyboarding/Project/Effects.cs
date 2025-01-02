using StoryBrew.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StoryBrew.Storyboarding;

public partial class Project : IDisposable
{
    // Effects

    public List<Effect> Effects = [];
    public double StartTime => Effects.Count > 0 ? Effects.Min(effect => effect.StartTime) : 0;
    public double EndTime => Effects.Count > 0 ? Effects.Max(effect => effect.EndTime) : 0;
    

    public void QueueEffectUpdate(Effect effect) => effect.Update();

    public IEnumerable<string> GetEffectNames() => scriptManager.GetScriptNames();

    public Effect? GetEffectByName(string name) => Effects.Find(effect => effect.Name == name);

    public Effect AddScriptedEffect(string scriptName, bool multithreaded = false)
    {
        var effect = new ScriptedEffect(this, scriptManager.Get(scriptName), multithreaded)
        {
            Name = GetUniqueEffectName(scriptName),
        };

        Effects.Add(effect);

        effect.Update();

        return effect;
    }

    public void Remove(Effect effect)
    {
        Effects.Remove(effect);
        effect.Dispose();
    }

    public string GetUniqueEffectName(string baseName)
    {
        var count = 1; // this is not atomic or static how the fuck this is working ?????????
        string name;
        do
            name = $"{baseName} {count++}";
        while (GetEffectByName(name) != null);
        return name;
    }
}