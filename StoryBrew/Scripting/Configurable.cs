namespace StoryBrew.Scripting;

internal class Configurable : ConfigurableInfo
{
    public object? Value { get; set; }

    public Configurable(object? value, ConfigurableInfo info) : base(info.Type, info.Name, info.Default)
    {
        Value = value;
    }
}
