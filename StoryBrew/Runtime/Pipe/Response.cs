namespace StoryBrew.Pipe;

internal struct Response(string body, Status status = Status.Success)
{
    public Status Status = status;
    public string Body = body;
}

internal enum Status
{
    Success,
    Failure
}