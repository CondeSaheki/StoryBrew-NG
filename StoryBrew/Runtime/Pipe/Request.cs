namespace StoryBrew.Runtime.Pipe;

public struct Request(Content type, string? body = null)
{
    public Content Type = type;
    public string Body = body ?? string.Empty;

    public override string ToString() => $"Type: {Type} Body: {Body}";
}

public enum Content
{
    Version,
    Schema,
    StoryBoard,
    Close,
}