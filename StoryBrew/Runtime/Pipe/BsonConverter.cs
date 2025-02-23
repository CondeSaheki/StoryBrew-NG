using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace StoryBrew.Runtime.Pipe;

internal static class BsonConverter
{
    public static Memory<byte> Encode<T>(in T content)
    {
        using MemoryStream memoryStream = new();
        using BsonDataWriter bsonWriter = new(memoryStream);

        JsonSerializer.CreateDefault().Serialize(bsonWriter, content);

        return memoryStream.GetBuffer().AsMemory(); // Note: Copy made
    }

    public static T Decode<T>(in byte[] data) where T : struct
    {
        using MemoryStream memoryStream = new(data, false);
        using BsonDataReader bsonReader = new(memoryStream);

        return JsonSerializer.CreateDefault().Deserialize<T>(bsonReader);
    }
}