using System.Security.Cryptography;
using System.Text;

public class HashPassword
{
    public string ComputeSha256Base64(string rawData)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(rawData.Trim());
        byte[] hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
