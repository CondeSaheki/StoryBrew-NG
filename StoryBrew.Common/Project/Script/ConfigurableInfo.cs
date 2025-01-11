using System.Reflection;

namespace StoryBrew;

internal class ConfigurableInfo
{
    public Type Type { get; }
    public string Name { get; }
    public object? Default { get; }

    public ConfigurableInfo(FieldInfo field, object instance)
    {
        Type = field.FieldType;
        Name = field.Name;
        Default = field.GetValue(instance);
    }

    protected ConfigurableInfo(Type type, string name, object? @default)
    {
        Name = name;
        Type = type;
        Default = @default;
    }
}
