// Note: The client only receive request's and only send response's.
// The server only send request's and only receive response's.
// Counterintuitive but is this way to give more autority to the server.

using System.IO.Pipes;
using System.IO.Pipelines;
using System.Buffers;
using StoryBrew.Runtime.LogSystem;

namespace StoryBrew.Runtime.Pipe;

public class Client : IDisposable
{
    private const string pipe_name = "storybrew_pipe";
    private readonly NamedPipeClientStream client;
    private readonly PipeReader reader;
    private readonly PipeWriter writer;

    private bool operating = false;

    public delegate Response RequestHandlerDelegate(Request request, ref bool working, CancellationToken cancellationToken = default);

    private RequestHandlerDelegate requestHandler;

    public Client(RequestHandlerDelegate requestHandler)
    {
        client = new(".", pipe_name, PipeDirection.InOut);
        reader = PipeReader.Create(client);
        writer = PipeWriter.Create(client);
        this.requestHandler = requestHandler;
    }

    public async Task Initialize(CancellationToken cancellationToken = default)
    {
        var working = true;
        while (!cancellationToken.IsCancellationRequested && working)
        {
            try
            {
                var request = await read(cancellationToken);
                var response = requestHandler(request, ref working, cancellationToken);
                await write(response, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception ex)
            {
                await write(new(ex.ToString(), Status.Failure), cancellationToken);
            }
        }
    }

    private async Task connectionCheck(CancellationToken cancellationToken = default)
    {
        if (operating)
        {
            if (client.IsConnected) return;
            throw new IOException("Connection lost.");
        }

        await client.ConnectAsync(cancellationToken).ConfigureAwait(false);
        operating = true;
    }

    private async Task write(Response response, CancellationToken cancellationToken = default)
    {
        await connectionCheck(cancellationToken).ConfigureAwait(false);

        var content = BsonConverter.Encode(response);
        var header = BitConverter.GetBytes(content.Length).AsMemory();
        var responseLength = header.Length + content.Length;
        var buffer = writer.GetMemory(responseLength);

        header.Span.CopyTo(buffer.Span);
        content.Span.CopyTo(buffer.Span[header.Length..]);

        writer.Advance(responseLength);
        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        Log.Debug($"response sent {response}");
    }

    private async Task<Request> read(CancellationToken cancellationToken)
    {
        await connectionCheck(cancellationToken).ConfigureAwait(false);

        const int header_length = sizeof(int);
        var headerReadResult = await reader.ReadAtLeastAsync(header_length, cancellationToken).ConfigureAwait(false);
        if (headerReadResult.Buffer.Length < header_length) throw new IOException("Incomplete read.");
        var headerBuffer = headerReadResult.Buffer.Slice(0, header_length);

        var contentLength = BitConverter.ToInt32(headerBuffer.FirstSpan);
        reader.AdvanceTo(headerBuffer.End);

        var contentReadResult = await reader.ReadAtLeastAsync(contentLength, cancellationToken).ConfigureAwait(false);
        if (contentReadResult.Buffer.Length < contentLength) throw new IOException("Incomplete read.");
        var contentBuffer = contentReadResult.Buffer.Slice(0, contentLength);

        var request = BsonConverter.Decode<Request>(contentBuffer.ToArray()); // Note: Copy made
        reader.AdvanceTo(contentBuffer.End);

        Log.Debug($"request received {request}");
        return request;
    }

    public void Dispose()
    {
        reader?.Complete();
        writer?.Complete();
        client?.Dispose();
    }
}