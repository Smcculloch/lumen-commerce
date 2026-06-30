namespace Lumen.Domain.Enums;

/// <summary>
/// Supported field types for template property definitions.
/// </summary>
public enum PropertyType
{
    Text,
    RichText,
    Html,
    Number,
    Integer,
    Decimal,
    Boolean,
    DateTime,
    Image,
    MediaReference,
    Reference,
    Select,
    MultiSelect,
    Json
}