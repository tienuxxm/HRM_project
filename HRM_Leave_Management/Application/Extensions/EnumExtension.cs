namespace Application.Extensions;

using System;
using System.ComponentModel;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());

        if (fieldInfo == null)
            return value.ToString(); // If no description attribute found, return the enum's name as a fallback.
        var attributes =
            (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

        return attributes.Length > 0
            ? attributes[0].Description
            : value.ToString(); // If no description attribute found, return the enum's name as a fallback.
    }
}