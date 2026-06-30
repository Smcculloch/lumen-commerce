namespace Lumen.Domain.Exceptions;

/// <summary>
/// Raised when template property values fail validation.
/// </summary>
public sealed class TemplateValidationException : Exception
{
    public TemplateValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more template property values are invalid.")
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}