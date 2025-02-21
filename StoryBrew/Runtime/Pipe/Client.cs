// Note: The client only receive request's and only send response's.
// The server only send request's and only receive response's.
// Counterintuitive but is this way to give more autority to the server.

using System.IO.Pipes;
using System.IO.Pipelines;
using System.Buffers;

namespace StoryBrew.Pipe;

internal class Client : IDisposable
{
    
    private const string pipe_name = "MyBsonPipe";
    private readonly NamedPipeClientStream client;
    private readonly PipeReader reader;
    private readonly PipeWriter writer;

    public bool? Connected = null;

    public Client()
    {
        client = new(".", pipe_name, PipeDirection.InOut);
        reader = PipeReader.Create(client);
        writer = PipeWriter.Create(client);
    }

    private async Task connect(CancellationToken cancellationToken = default)
    {
        await client.ConnectAsync(cancellationToken)
            .ContinueWith((_) =>
            {
                if (!client.IsConnected) throw new Exception("Failed to connect");
                Connected = true;
            }, cancellationToken)
            .ConfigureAwait(false);
    }
    public void Send(in Response response, CancellationToken cancellationToken = default)
    {
        var content = BsonConverter.Encode(response);
        SendRaw(content, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async Task SendRaw(Memory<byte> content, CancellationToken cancellationToken)
    {
        if (!Connected ?? false) await connect(cancellationToken).ConfigureAwait(false);

        var header = BitConverter.GetBytes(content.Length).AsMemory();

        var buffer = writer.GetMemory(header.Length + content.Length);

        header.Span.CopyTo(buffer.Span);
        content.Span.CopyTo(buffer.Span[header.Length..]);

        writer.Advance(header.Length + content.Length);
        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
    public Request Receive(CancellationToken cancellationToken = default)
    {
        var memory = ReceiveRaw(cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        return BsonConverter.Decode<Request>(memory.ToArray()); // Note: Copy made
    }

    public async Task<Memory<byte>> ReceiveRaw(CancellationToken cancellationToken)
    {
        if (!client.IsConnected) await connect(cancellationToken).ConfigureAwait(false);

        var taskResult = await reader.ReadAtLeastAsync(sizeof(int), cancellationToken).ConfigureAwait(false);
        var lengthBuffer = taskResult.Buffer.Slice(0, sizeof(int));

        var contentLength = BitConverter.ToInt32(lengthBuffer.FirstSpan);

        reader.AdvanceTo(lengthBuffer.End);

        var taskResult2 = await reader.ReadAtLeastAsync(contentLength, cancellationToken).ConfigureAwait(false);
        var data = taskResult2.Buffer.Slice(0, contentLength);

        var result = data.ToArray().AsMemory(); // Note: copy made

        reader.AdvanceTo(data.End);

        return result;
    }

    public void Disconnect()
    {
        Connected = false;
        Dispose();
    }

    public void Dispose()
    {
        client?.Dispose();
    }
}