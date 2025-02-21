using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace StoryBrew.Pipe;

internal static class BsonConverter
{
    public static Memory<byte> Encode<T>(in T content)
    {
        using MemoryStream memoryStream = new();
        using BsonDataWriter bsonWriter = new(memoryStream);

        JsonSerializer.CreateDefault().Serialize(bsonWriter, content);

        return memoryStream.GetBuffer().AsMemory();
    }

    public static T Decode<T>(in byte[] data) where T : struct
    {
        using MemoryStream memoryStream = new(data, false);
        using BsonDataReader bsonReader = new(memoryStream);

        return JsonSerializer.CreateDefault().Deserialize<T>(bsonReader);
    }
}




/*
.
├── Runtime // now contains all sensible and internal code
│   ├── Bootstrap.cs
│   ├── Bootstrap.New.cs
│   ├── Bootstrap.Pipe.cs
│   ├── Bootstrap.Run.cs
│   ├── Bootstrap.Schema.cs
│   ├── Bootstrap.Version.cs
│   ├── Log.cs
│   ├── Pipe
│   │   ├── BsonConverter.cs
│   │   ├── Client.cs
│   │   ├── Request.cs
│   │   ├── Response.cs
│   │   └── Server.cs
│   ├── ProjectData.cs
│   └── Version.cs
├── Script // unsure about name and structure
│   ├── Core // agragated all "core" code needed by Script and its dependencies, not sure if this is good
│   │   ├── Anchor.cs
│   │   ├── Commandable.cs
│   │   ├── Command.cs
│   │   ├── Easing.cs
│   │   ├── Group.cs
│   │   ├── ICommand.cs
│   │   ├── IElement.cs
│   │   ├── Layer.cs
│   │   └── Writable.cs
│   ├── Element 
│   │   ├── Animation.cs
│   │   ├── Common // unsure about naming, it contains "optional" elements for "advanced users", I think it should be separated from the most commonly used code
│   │   │   ├── Container.cs
│   │   │   ├── ElementPool.cs
│   │   │   ├── Segment.cs
│   │   │   └── Text // not implemented 
│   │   │       ├── Core
│   │   │       │   └── Generator.cs
│   │   │       └── Text.cs
│   │   ├── Raw.cs
│   │   ├── Sample.cs
│   │   ├── Sprite.cs
│   │   ├── Transform // found a more appropriate naming, not sure if should be inside Element folder or not. 
│   │   │   ├── Alpha.cs
│   │   │   ├── Angle.cs
│   │   │   ├── Blending.cs
│   │   │   ├── Colour.cs
│   │   │   ├── FlipH.cs
│   │   │   ├── FlipV.cs
│   │   │   ├── Loop.cs
│   │   │   ├── Position.cs
│   │   │   ├── PositionX.cs
│   │   │   ├── PositionY.cs
│   │   │   ├── Scale.cs
│   │   │   ├── ScaleVector.cs
│   │   │   └── Trigger.cs
│   │   └── Video.cs
│   ├── Element3D // to be implemented in the future
│   ├── Script.cs // the central abstract class of the project, the user will the using this class extensively. Need to be feed mainly with IElement's   
│   └── Utilities // did not change mutch of this folder, code inside it needs to be analysed and refactored, its made to be used in user script implementations but are not direct dependencies
│       ├── Animations // might be removed
│       │   ├── EasingFunctions.cs
│       │   ├── InterpolatingFunctions.cs
│       │   ├── Keyframe.cs
│       │   ├── KeyframedValue.cs
│       │   └── KeyframedValueExtensions.cs
│       ├── Extensions 
│       │   ├── Color4Extensions.cs
│       │   ├── SKColorExtensions.cs
│       │   └── StreamReaderExtensions.cs // might be removed 
│       ├── FftStream.cs // obsolete
│       ├── Mapset // obsolete 
│       │   ├── Beatmap.cs 
│       │   ├── BeatmapExtensions.cs
│       │   ├── ControlPoint.cs
│       │   ├── Curves
│       │   │   ├── BaseCurve.cs
│       │   │   ├── BezierCurve.cs
│       │   │   ├── CatmullCurve.cs
│       │   │   ├── CircleCurve.cs
│       │   │   ├── CompositeCurve.cs
│       │   │   ├── Curve.cs
│       │   │   └── TransformedCurve.cs
│       │   ├── Break.cs
│       │   ├── Circle.cs
│       │   ├── HitObject.cs
│       │   ├── Hold.cs
│       │   ├── Slider.cs
│       │   └── Spinner.cs
│       ├── Subtitles // marked to be refactored 
│       │   ├── Parsers
│       │   │   ├── AssParser.cs
│       │   │   ├── SbvParser.cs
│       │   │   └── SrtParser.cs
│       │   ├── SubtitleLine.cs
│       │   └── SubtitleSet.cs
│       └── UnityConverter.cs // marked to be refactored 
└── StoryBrew.csproj


*/