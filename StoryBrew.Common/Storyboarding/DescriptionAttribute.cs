﻿namespace StoryBrew.Common.Storyboarding;

[AttributeUsage(AttributeTargets.Field)]
public class DescriptionAttribute : Attribute
{
    public string Content { get; set; }

    public DescriptionAttribute(string content)
    {
        Content = content;
    }
}
