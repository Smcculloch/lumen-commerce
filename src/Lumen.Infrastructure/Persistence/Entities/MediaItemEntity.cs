namespace Lumen.Infrastructure.Persistence.Entities;

/// <summary>
/// EF persistence model for <see cref="Domain.Media.MediaItem"/>.
/// </summary>
public sealed class MediaItemEntity
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string PublicUrl { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string? AltText { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}