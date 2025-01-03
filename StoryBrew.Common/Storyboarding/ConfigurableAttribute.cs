﻿namespace StoryBrew.Common.Storyboarding;

[AttributeUsage(AttributeTargets.Field)]
public class ConfigurableAttribute : Attribute
{
    public string DisplayName { get; set; } = string.Empty;
}
