namespace StoryBrew.Scripting;

[AttributeUsage(AttributeTargets.Field)]
public class ConfigurableAttribute : Attribute
{
    // public string DisplayName { get; set; } = string.Empty;
}

/*
    [AttributeUsage(AttributeTargets.Field)]
    public class ConfigurableAttribute : Attribute
    {
        public object? Default { get; }

        public ConfigurableAttribute(object defaultValue) { Default = defaultValue; }

        public ConfigurableAttribute(Type type, object?[]? args = null) { Default = Activator.CreateInstance(type, args); }
    }
*/
