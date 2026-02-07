using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RePlay.Application.Interfaces;

namespace RePlay.Infrastructure.Services;

public class FileUploadService : IFileUploadService
{
    private readonly FileUploadSettings _settings;
    private readonly ILogger<FileUploadService> _logger;
    private readonly string _basePath;

    public FileUploadService(
        IOptions<FileUploadSettings> settings,
        ILogger<FileUploadService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        // Set base path relative to the application root
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), _settings.UploadPath);

        // Ensure upload directory exists
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
            _logger.LogInformation("Created upload directory: {Path}", _basePath);
        }
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType)
    {
        // Validate file extension
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_settings.AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", _settings.AllowedExtensions)}");
        }

        // Validate file size
        if (fileStream.Length > _settings.MaxFileSizeBytes)
        {
            throw new ArgumentException($"File size exceeds maximum allowed size of {_settings.MaxFileSizeBytes / (1024 * 1024)}MB");
        }

        // Validate content type
        var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedContentTypes.Contains(contentType.ToLower()))
        {
            throw new ArgumentException($"Content type '{contentType}' is not allowed");
        }

        // Generate unique filename
        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";

        // Create year/month subdirectory for organization
        var dateFolder = DateTime.UtcNow.ToString("yyyy/MM");
        var targetDirectory = Path.Combine(_basePath, dateFolder);

        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        var filePath = Path.Combine(targetDirectory, uniqueFileName);
        var relativePath = Path.Combine(_settings.UploadPath, dateFolder, uniqueFileName).Replace("\\", "/");

        // Save the file
        await using var outputStream = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(outputStream);

        _logger.LogInformation("Uploaded image: {FileName} -> {Path}", fileName, relativePath);

        return relativePath;
    }

    public Task<bool> DeleteImageAsync(string imagePath)
    {
        try
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), imagePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Deleted image: {Path}", imagePath);
                return Task.FromResult(true);
            }

            _logger.LogWarning("Image not found for deletion: {Path}", imagePath);
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image: {Path}", imagePath);
            return Task.FromResult(false);
        }
    }

    public string GetImageUrl(string imagePath)
    {
        // Return the relative path which can be served by static files middleware
        return $"/{imagePath}";
    }
}
