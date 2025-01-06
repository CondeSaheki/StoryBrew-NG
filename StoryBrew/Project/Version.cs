using Newtonsoft.Json;

namespace StoryBrew;

[JsonConverter(typeof(VersionJsonConverter))]
public class Version
{
    public uint Major { get; init; }
    public uint Minor { get; init; }
    public uint Patch { get; init; }

    public Version(uint major, uint minor, uint patch)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
    }

    public Version(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Version string cannot be null or empty.", nameof(value));

        var slices = value.Split('.');
        if (slices.Length != 3)
            throw new ArgumentException("Version string must have exactly three components (e.g., '1.0.0').", nameof(value));

        Major = uint.Parse(slices[0]);
        Minor = uint.Parse(slices[1]);
        Patch = uint.Parse(slices[2]);
    }

    public static Version FromJsonFile(string path)
    {
        var json = JsonConvert.DeserializeObject<Versionedjson>(File.ReadAllText(path))
            ?? throw new InvalidOperationException("Failed to deserialize versioned file.");
        return json.Version;
    }

    private class Versionedjson
    {
        public Version Version = new(null);
    }

    public override string ToString() => $"{Major}.{Minor}.{Patch}";
    public (uint Major, uint Minor, uint Patch) ToTuple() => (Major, Minor, Patch);

    public override bool Equals(object? obj) =>
        obj is Version other && Major == other.Major && Minor == other.Minor && Patch == other.Patch;

    public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch);

    public int CompareTo(Version? other)
    {
        if (other is null) return 1;

        int majorComparison = Major.CompareTo(other.Major);
        if (majorComparison != 0) return majorComparison;

        int minorComparison = Minor.CompareTo(other.Minor);
        if (minorComparison != 0) return minorComparison;

        return Patch.CompareTo(other.Patch);
    }
}

public class VersionJsonConverter : JsonConverter<Version>
{
    public override Version? ReadJson(JsonReader reader, Type objectType, Version? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType != JsonToken.String) throw new JsonSerializationException("Expected a string for version.");

        return new Version(reader.Value?.ToString());
    }

    public override void WriteJson(JsonWriter writer, Version? value, JsonSerializer serializer)
    {
        if (value is null) writer.WriteNull();
        else writer.WriteValue(value.ToString());
    }
}
