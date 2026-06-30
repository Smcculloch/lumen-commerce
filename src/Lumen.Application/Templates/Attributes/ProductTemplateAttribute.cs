namespace Lumen.Application.Templates.Attributes;

/// <summary>
/// Marks a class as a code-first PIM product template discovered via reflection.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ProductTemplateAttribute : Attribute
{
    public ProductTemplateAttribute(string key, string displayName)
    {
        Key = key;
        DisplayName = displayName;
    }

    public string Key { get; }
    public string DisplayName { get; }
    public string? Description { get; init; }
}