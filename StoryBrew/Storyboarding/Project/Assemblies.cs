
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace StoryBrew.Storyboarding;

public partial class Project : IDisposable
{
    // Assemblies

    public static string GetRuntimeRefDirectory()
    {
        // C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.5 => C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.5\ref\net8.0
        return Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "..", "..", "..",
            "packs",
            "Microsoft.NETCore.App.Ref",
            RuntimeEnvironment.GetSystemVersion().TrimStart('v'), // eg. 8.0.5
            "ref",
            "net" + RuntimeEnvironment.GetSystemVersion().Substring(1, 3) // eg. net8.0
        );
    }

    private static readonly string[] netRuntimeAssemblies =
        Directory.GetFiles(GetRuntimeRefDirectory(), "*.dll")
        .Where(x => !Path.GetFileName(x).EndsWith(".Native.dll")).ToArray();

    private static readonly List<string> defaultAssemblies = new List<string>()
        {
            "/mnt/cea0b7f8-1920-4cdd-acfa-2a7bb8dd8960/programing/osu/storybrewEEEE/editor/bin/Debug/net9.0/OpenTK.Mathematics.dll", // OpenTK
            "/mnt/cea0b7f8-1920-4cdd-acfa-2a7bb8dd8960/programing/osu/storybrewEEEE/editor/bin/Debug/net9.0/SkiaSharp.dll", // Skia
            
            typeof(StoryBrew.Common.Scripting.Script).Assembly.Location // StorybrewCommon.dll
        }.Concat(netRuntimeAssemblies).ToList();

    public static IEnumerable<string> DefaultAssemblies => defaultAssemblies;

    private List<string> importedAssemblies = new List<string>();
    public IEnumerable<string> ImportedAssemblies
    {
        get { return importedAssemblies; }
        set
        {
            importedAssemblies = new List<string>(value);
            scriptManager.ReferencedAssemblies = ReferencedAssemblies;
            
        }
    }

    public IEnumerable<string> ReferencedAssemblies
        => DefaultAssemblies.Concat(importedAssemblies);
}