using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using StoryBrew.Runtime.LogSystem;

namespace StoryBrew.Runtime;

public partial class Bootstrap
{
    private VersionInfo getVersionInfo()
    {
        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(getSchema())));
        return new()
        {
            Version = VERSION,
            BuildId = getBuildId().ToString(),
            Hash = hash
        };
    }

    private void handleVersionCommand() => Log.Message($"StoryBrew {getVersionInfo()}");
}

public readonly struct VersionInfo
{
    public Version Version { get; init; }
    public string Hash { get; init; }
    public string BuildId { get; init; }

    public override string ToString() => $"Version {Version}, BuildId {BuildId}, Hash {Hash}";
}
