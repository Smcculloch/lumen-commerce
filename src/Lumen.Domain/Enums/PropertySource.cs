namespace Lumen.Domain.Enums;

/// <summary>
/// Indicates where a property definition originated.
/// </summary>
public enum PropertySource
{
    /// <summary>Compiled into the application from C# template definitions.</summary>
    Code,

    /// <summary>Added via backoffice to extend a code-defined template.</summary>
    Extension,

    /// <summary>Part of a fully dynamic template created only in the backoffice.</summary>
    Dynamic
}