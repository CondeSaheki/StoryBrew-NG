using StoryBrew.Runtime.LogSystem;

namespace StoryBrew.Runtime;

public partial class Bootstrap
{
    private void handlePipeCommand()
    {
        var client = new Pipe.Client();
        var working = true;
        while (working)
        {
            using Collector collector = new();
            try
            {
                var request = client.Receive();

                var response = request.Type switch
                {
                    Pipe.Content.Version => handleVersionRequest(request.Body),
                    Pipe.Content.Schema => handleSchemaRequest(request.Body),
                    Pipe.Content.StoryBoard => handleRunRequest(request.Body),
                    Pipe.Content.Close => handleCloseRequest(request.Body, ref working),
                    _ => throw new Exception("Unhandled request type"),
                };

                client.Send(response);
            }
            catch (Exception ex)
            {
                string message = $"Log:\n{collector.Consume()}\nException: {ex.Message}";
                client.Send(new(message, Pipe.Status.Failure));
                client.Disconnect();
                break;
            }
        }
        client.Dispose();
    }

    private static Pipe.Response handleCloseRequest(ReadOnlySpan<char> requestBody, ref bool working)
    {
        if (requestBody.Length != 0) Log.Debug(requestBody.ToString());

        working = false;
        return new(string.Empty);
    }
}