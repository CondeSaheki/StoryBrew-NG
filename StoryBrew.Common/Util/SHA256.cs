using System.Security.Cryptography;
using System.Text;

namespace StoryBrew.Util;

internal static partial class Helper
{
    public static string SHA256String(string value)
    {
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(value)))
        {
            byte[] hash = SHA256.HashData(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
            
    public static async Task<string> SHA256StringAsync(string value, CancellationToken cancellationToken = default)
    {
        await using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(value)))
        {
            byte[] hash = await SHA256.HashDataAsync(stream, cancellationToken);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    public static string SHA256File(string filePath)
    {
        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            byte[] hash = SHA256.HashData(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    public static async Task<string> SHA256FileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            byte[] hash = await SHA256.HashDataAsync(stream, cancellationToken);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
