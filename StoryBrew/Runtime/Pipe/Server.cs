// Note: The client only receive request's and only send response's.
// The server only send request's and only receive response's.
// Counterintuitive but is this way to give more autority to the server.

using System.IO.Pipes;
using System.IO.Pipelines;
using System.Buffers;
using StoryBrew.Runtime.LogSystem;

namespace StoryBrew.Runtime.Pipe;

public class Server : IDisposable
{
    private const string pipe_name = "storybrew_pipe";
    private readonly NamedPipeServerStream server;
    private readonly PipeReader reader;
    private readonly PipeWriter writer;

    private bool operating = false;

    public Server()
    {
        server = new(pipe_name, PipeDirection.InOut);
        reader = PipeReader.Create(server);
        writer = PipeWriter.Create(server);
    }

    /// <summary>
    /// Sends the specified request to the client, and returns the response received
    /// from the client. Only call this method synchronously.
    /// </summary>
    /// <param name="request">The request to send to the client.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The response received from the client.</returns>
    public async Task<Response> Request(Request request, CancellationToken cancellationToken = default)
    {
        await connectionCheck(cancellationToken).ConfigureAwait(false);
        await write(request, cancellationToken).ConfigureAwait(false);
        return await read(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends the specified request to the client, and returns the response received
    /// from the client. If the request fails, the server will attempt to send a close
    /// request to the client.
    /// </summary>
    /// <param name="request">The request to send to the client.</param>
    /// <param name="content">The content received from the client, or null if the request failed.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the request was successful, false otherwise.</returns>
    public bool TryRequest(Request request, out string content, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = Request(request, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
            if (response.Status != Status.Failure)
            {
                content = response.Body;
                return true;
            }

            Log.Debug($"Request Failure: {response}.");
            Close();
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception ex)
        {
            Log.Error($"Unhandled exception when trying to send request: {request} exception: {ex}");
        }
        content = string.Empty;
        return false;
    }

    /// <summary>
    /// sends a close request with the optional message and closes the pipe.
    /// </summary>
    /// <param name="message">The message to send in the close request. Defaults to null.</param>
    public void Close(string? message = null)
    {
        try
        {
            if (!operating || !server.IsConnected || server == null) return;

            write(new(Content.Close, message)).ConfigureAwait(false).GetAwaiter().GetResult();
            var response = read().ConfigureAwait(false).GetAwaiter().GetResult();

            server.Disconnect();

            if (response.Status == Status.Failure) throw new Exception($"Request closure: {response}");
        }
        catch (Exception ex)
        {
            Log.Error($"Close was not completed gracefully: {ex}");
        }
        finally
        {
            writer?.Complete();
            reader?.Complete();
            server?.Dispose();
        }
    }

    private async Task connectionCheck(CancellationToken cancellationToken = default)
    {
        if (operating)
        {
            if (server.IsConnected) return;
            throw new IOException("Connection lost.");
        }

        await server.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
        operating = true;
    }

    private async Task write(Request request, CancellationToken cancellationToken = default)
    {
        var content = BsonConverter.Encode(request);
        var header = BitConverter.GetBytes(content.Length).AsMemory();
        var requestSize = header.Length + content.Length;
        var buffer = writer.GetMemory(requestSize);

        header.Span.CopyTo(buffer.Span);
        content.Span.CopyTo(buffer.Span[header.Length..]);

        writer.Advance(requestSize);
        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

        Log.Debug($"request sent {request}");
    }

    private async Task<Response> read(CancellationToken cancellationToken = default)
    {
        // header

        const int header_length = sizeof(int);
        var headerReadResult = await reader.ReadAtLeastAsync(header_length, cancellationToken).ConfigureAwait(false);
        if (headerReadResult.Buffer.Length < header_length) throw new IOException("Incomplete read.");
        var headerBuffer = headerReadResult.Buffer.Slice(0, header_length);

        var contentLength = BitConverter.ToInt32(headerBuffer.FirstSpan);
        reader.AdvanceTo(headerBuffer.End);

        // content

        var contentReadResult = await reader.ReadAtLeastAsync(contentLength, cancellationToken).ConfigureAwait(false);
        if (contentReadResult.Buffer.Length < contentLength) throw new IOException("Incomplete read.");
        var contentBuffer = contentReadResult.Buffer.Slice(0, contentLength);

        var response = BsonConverter.Decode<Response>(contentBuffer.ToArray()); // Note: Copy made
        reader.AdvanceTo(contentBuffer.End);

        Log.Debug($"response received {response}");
        return response;
    }

    public void Dispose() => Close();
}