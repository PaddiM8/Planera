namespace Planera.Api.Utility;

public static class EmailTemplateUtility
{
    public static string Button(string value, string href, bool isPrimary)
    {
        var backgroundColor = isPrimary ? "#1d4ed8" : "#ffffff";
        var foregroundColor = isPrimary ? "#ffffff" : "#000000";

        return $"""
                 <a href="{href}" style="display: inline-block; background-color: {backgroundColor}; text-decoration: none; color: {foregroundColor}; border: 1px solid #d4d4d4; font-weight: 600; padding: 8px 12px; margin-top: -8px; margin-right: 8px; border-radius: 8px;">{value}</a>
                 """;
    }
}