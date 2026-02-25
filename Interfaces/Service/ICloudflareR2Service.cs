namespace API_PortalSantosTech.Interfaces;

public interface ICloudflareR2Service
{
    Task<string> UploadBase64ImageAsync(string base64Image, string folder);
}
