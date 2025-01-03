﻿using StoryBrew.Common.Util;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StoryBrew.Common.Storyboarding;

public class EffectConfig
{
    private readonly Dictionary<string, ConfigField> fields = [];

    public int FieldCount => fields.Count;
    public IEnumerable<ConfigField> Fields => fields.Values;
    public IEnumerable<ConfigField> SortedFields
    {
        get
        {
            var sortedValues = new List<ConfigField>(fields.Values);
            sortedValues.Sort((first, second) => first.Order - second.Order);
            return sortedValues;
        }
    }

    public string[] FieldNames
    {
        get
        {
            var names = new string[fields.Keys.Count];
            fields.Keys.CopyTo(names, 0);
            return names;
        }
    }

    public void UpdateField(string name, string displayName, string? description, int order, Type fieldType, object defaultValue, NamedValue[] allowedValues, string? beginsGroup)
    {
        if (fieldType == null)
            return;

        if (displayName == null)
        {
            displayName = Regex.Replace(name, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2");
            displayName = Regex.Replace(displayName, @"(\p{Ll})(\P{Ll})", "$1 $2");
        }

        var value = fields.TryGetValue(name, out ConfigField field) ?
            convertFieldValue(field.Value, field.Type, fieldType, defaultValue) :
            defaultValue;

        var isAllowed = allowedValues == null;
        if (!isAllowed && allowedValues != null)
        {
            foreach (var allowedValue in allowedValues)
                if (value.Equals(allowedValue.Value))
                {
                    isAllowed = true;
                    break;
                }
        }
        if (!isAllowed)
            value = defaultValue;

        fields[name] = new ConfigField()
        {
            Name = name,
            DisplayName = displayName,
            Description = description?.Trim() ?? string.Empty,
            Value = value,
            Type = fieldType,
            AllowedValues = allowedValues ?? throw new ArgumentNullException(nameof(allowedValues)),
            BeginsGroup = beginsGroup,
            Order = order,
        };
    }

    public void RemoveField(string name)
        => fields.Remove(name);


    public bool SetValue(string name, object value)
    {
        var field = fields[name];
        if (field.Value == value)
            return false;

        fields[name] = new ConfigField()
        {
            Name = field.Name,
            DisplayName = field.DisplayName,
            Description = field.Description,
            Value = value,
            Type = field.Type,
            AllowedValues = field.AllowedValues,
            BeginsGroup = field.BeginsGroup,
            Order = field.Order,
        };
        return true;
    }

    public object GetValue(string name)
        => fields[name].Value;

    private object convertFieldValue(object value, Type oldType, Type newType, object defaultValue)
    {
        if (newType.IsAssignableFrom(oldType))
            return value;

        try
        {
            return Convert.ChangeType(value, newType, CultureInfo.InvariantCulture);
        }
        catch
        {
            return defaultValue;
        }
    }

    public struct ConfigField
    {
        public string Name;
        public string DisplayName;
        public string Description;
        public object Value;
        public Type Type;
        public NamedValue[] AllowedValues;
        public string? BeginsGroup;
        public int Order;
    }
}
