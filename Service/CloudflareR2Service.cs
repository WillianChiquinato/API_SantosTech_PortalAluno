using System.Text.RegularExpressions;
using Amazon.S3;
using Amazon.S3.Model;
using API_PortalSantosTech.Interfaces;

namespace API_PortalSantosTech.Services;

public class CloudflareR2Service : ICloudflareR2Service
{
    private static readonly Regex DataUriRegex = new(
        @"^data:(?<mime>[\w\-\.]+\/[\w\-\.+]+);base64,(?<data>.+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _publicUrl;

    public CloudflareR2Service(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
        _bucketName = Environment.GetEnvironmentVariable("CLOUDFLARE_BUCKET_NAME")
                      ?? throw new InvalidOperationException("Variável CLOUDFLARE_BUCKET_NAME não configurada.");
        _publicUrl = Environment.GetEnvironmentVariable("CLOUDFLARE_PUBLIC_URL")
                     ?? throw new InvalidOperationException("Variável CLOUDFLARE_PUBLIC_URL não configurada.");
    }

    public async Task<string> UploadBase64ImageAsync(string base64Image, string folder)
    {
        if (string.IsNullOrWhiteSpace(base64Image))
            throw new ArgumentException("Imagem em base64 não informada.", nameof(base64Image));

        var parsed = ParseBase64Image(base64Image.Trim());

        var normalizedFolder = folder.Trim().Trim('/');
        var objectKey =
            $"{normalizedFolder}/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}{parsed.Extension}";

        await using var imageStream = new MemoryStream(parsed.Bytes);

        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            InputStream = imageStream,
            ContentType = parsed.ContentType,
            AutoCloseStream = true,
            UseChunkEncoding = false,
            DisablePayloadSigning = true
        };

        await _s3Client.PutObjectAsync(putRequest);

        return $"{_publicUrl.TrimEnd('/')}/{objectKey}";
    }

    private static (byte[] Bytes, string ContentType, string Extension) ParseBase64Image(string input)
    {
        var dataUriMatch = DataUriRegex.Match(input);
        if (dataUriMatch.Success)
        {
            var contentType = dataUriMatch.Groups["mime"].Value;
            var data = dataUriMatch.Groups["data"].Value;

            return (
                Convert.FromBase64String(data),
                contentType,
                GetExtensionFromContentType(contentType));
        }

        return (
            Convert.FromBase64String(input),
            "image/jpeg",
            ".jpg");
    }

    private static string GetExtensionFromContentType(string contentType)
    {
        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            "image/svg+xml" => ".svg",
            _ => ".bin"
        };
    }
}
