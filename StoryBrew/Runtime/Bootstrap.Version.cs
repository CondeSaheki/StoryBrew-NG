using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using StoryBrew.Runtime.LogSystem;

namespace StoryBrew.Runtime;

public partial class Bootstrap
{
    private readonly struct VersionInfo
    {
        public string Version { get; init; }
        public string Hash { get; init; }
        public string BuildId { get; init; }

        public override string ToString() => $"Version {Version}, BuildId {BuildId}, Hash {Hash}";
    }
    
    private VersionInfo getVersionInfo()
    {
        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(getSchema())));
        return new()
        {
            Version = VERSION.ToString(),
            BuildId = getBuildId().ToString(),
            Hash = hash
        };
    }

    private void handleVersionCommand() => Log.Message($"StoryBrew {getVersionInfo()}");
    
    private Pipe.Response handleVersionRequest(ReadOnlySpan<char> requestBody)
    {
        if (requestBody.Length != 0) throw new InvalidOperationException("Unexpected request body");

        var body = JsonConvert.SerializeObject(getVersionInfo(), Formatting.None);
        return new(body);
    }
}