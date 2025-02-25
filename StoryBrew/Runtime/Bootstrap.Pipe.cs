using Newtonsoft.Json;
using StoryBrew.Runtime.LogSystem;

namespace StoryBrew.Runtime;

public partial class Bootstrap
{
    private void handlePipeCommand()
    {
        CancellationTokenSource tokenSource = new();
        tokenSource.CancelAfter(TimeSpan.FromMinutes(5));

        using var client = new Pipe.Client(requestHandler);
        client.Initialize(tokenSource.Token).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    private Pipe.Response requestHandler(Pipe.Request request, ref bool working, CancellationToken cancellationToken = default)
    {
        using Collector collector = new();
        try
        {
            return request.Type switch
            {
                Pipe.Content.Version => handleVersionRequest(request.Body),
                Pipe.Content.Schema => handleSchemaRequest(request.Body),
                Pipe.Content.StoryBoard => handleRunRequest(request.Body),
                Pipe.Content.Close => handleCloseRequest(request.Body, ref working),
                _ => throw new Exception("Unhandled request type"),
            };
        }
        catch (Exception ex)
        {
            var message = $"An error occurred while processing the request.\nLog Details:\n{collector.Consume()}\nError Message: {ex.Message}";
            return new(message, Pipe.Status.Failure);
        }
    }

    private Pipe.Response handleVersionRequest(ReadOnlySpan<char> requestBody)
    {
        if (requestBody.Length != 0) throw new InvalidOperationException("Unexpected request body");

        var body = JsonConvert.SerializeObject(getVersionInfo(), Formatting.None);
        return new(body);
    }

    private Pipe.Response handleSchemaRequest(ReadOnlySpan<char> requestBody)
    {
        if (requestBody.Length != 0) throw new InvalidOperationException("Unexpected request body");

        return new(getSchema(false));
    }

    private Pipe.Response handleRunRequest(in string requestBody)
    {
        // Note: The response is not correctly implemented
        using Collector collector = new();
        var result = processProject(ProjectData.FromString(requestBody));
        Log.Warnning("Todo: implement response that contains the result and the log:\n" + collector.Consume());
        return new(result);
    }

    private static Pipe.Response handleCloseRequest(ReadOnlySpan<char> requestBody, ref bool working)
    {
        if (requestBody.Length != 0) Log.Debug(requestBody.ToString());

        working = false;
        return new(string.Empty);
    }
}
