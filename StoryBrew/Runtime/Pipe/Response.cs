namespace StoryBrew.Runtime.Pipe;

public struct Response(string body, Status status = Status.Success)
{
    public Status Status = status;
    public string Body = body;

    public override string ToString() => $"{Status} : {Body}";
}

public enum Status
{
    Success,
    Failure
}