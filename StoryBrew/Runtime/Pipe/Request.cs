namespace StoryBrew.Pipe;

internal struct Request(string body, Content type)
{
    public Content Type = type;
    public string Body = body;
}

internal enum Content
{
    Version,
    Schema,
    StoryBoard,
    Close,
}