namespace RePlay.Application.Interfaces;

public interface IFileUploadService
{
    Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType);
    Task<bool> DeleteImageAsync(string imagePath);
    string GetImageUrl(string imagePath);
}

public class FileUploadSettings
{
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024; // 5MB default
    public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".webp" };
    public string UploadPath { get; set; } = "uploads/images";
}
