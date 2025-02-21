// Note: The client only receive request's and only send response's.
// The server only send request's and only receive response's.
// Counterintuitive but is this way to give more autority to the server.

using System.IO.Pipes;
using System.IO.Pipelines;
using System.Buffers;

namespace StoryBrew.Pipe;

internal class PipeServer : IDisposable
{
    private const string pipe_name = "MyBsonPipe";
    private readonly NamedPipeServerStream server;
    private readonly PipeReader reader;
    private readonly PipeWriter writer;

    public PipeServer()
    {
        server = new(pipe_name, PipeDirection.InOut, 0, PipeTransmissionMode.Byte, System.IO.Pipes.PipeOptions.Asynchronous);
        reader = PipeReader.Create(server);
        writer = PipeWriter.Create(server);
    }

    public async Task<Memory<byte>> Receive(CancellationToken cancellationToken = default)
    {
        if (!server.IsConnected) await connect(cancellationToken).ConfigureAwait(false);

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

    public async Task Send(Memory<byte> content, CancellationToken cancellationToken = default)
    {
        if (!server.IsConnected) await connect(cancellationToken).ConfigureAwait(false);

        var header = BitConverter.GetBytes(content.Length).AsMemory();

        var buffer = writer.GetMemory(header.Length + content.Length);

        header.Span.CopyTo(buffer.Span);
        content.Span.CopyTo(buffer.Span[header.Length..]);

        writer.Advance(header.Length + content.Length);
        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task connect(CancellationToken cancellationToken = default)
    {
        await server.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
    }

    public void Disconnect(CancellationToken cancellationToken = default) => server.Disconnect();

    public void Dispose()
    {
        server?.Dispose();
    }
}