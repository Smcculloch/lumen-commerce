namespace Lumen.Domain.Media;

/// <summary>
/// A file stored in the media library.
/// </summary>
public sealed class MediaItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string FileName { get; private set; } = string.Empty;
    public string StoragePath { get; private set; } = string.Empty;
    public string PublicUrl { get; private set; } = string.Empty;
    public string MimeType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public string? AltText { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static MediaItem Create(
        string fileName,
        string storagePath,
        string publicUrl,
        string mimeType,
        long sizeBytes,
        string? altText = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(publicUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(mimeType);

        return new MediaItem
        {
            FileName = fileName.Trim(),
            StoragePath = storagePath.Trim(),
            PublicUrl = publicUrl.Trim(),
            MimeType = mimeType.Trim(),
            SizeBytes = sizeBytes,
            AltText = altText?.Trim()
        };
    }

    public static MediaItem Rehydrate(
        Guid id,
        string fileName,
        string storagePath,
        string publicUrl,
        string mimeType,
        long sizeBytes,
        string? altText,
        DateTimeOffset createdAt) =>
        new()
        {
            Id = id,
            FileName = fileName,
            StoragePath = storagePath,
            PublicUrl = publicUrl,
            MimeType = mimeType,
            SizeBytes = sizeBytes,
            AltText = altText,
            CreatedAt = createdAt
        };

    public void UpdateAltText(string? altText)
    {
        AltText = altText?.Trim();
    }
}