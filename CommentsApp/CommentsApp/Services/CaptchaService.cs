using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Extensions.Caching.Distributed;

public class CaptchaService
{
    private readonly IDistributedCache _cache;
    private static readonly Random _random = new();

    public CaptchaService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public (byte[] Image, string Id) GenerateCaptcha()
    {
        var code = GenerateCode();
        var id = Guid.NewGuid().ToString();

        // сохраняем код в Redis (или в памяти, если без Redis)
        _cache.SetString(id, code, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });

        var image = GenerateImage(code);
        return (image, id);
    }

    public bool ValidateCaptcha(string id, string input)
    {
        var code = _cache.GetString(id);
        if (code == null) return false;
        return string.Equals(code, input, StringComparison.OrdinalIgnoreCase);
    }

    private static string GenerateCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        return new string(Enumerable.Range(0, 5)
            .Select(_ => chars[_random.Next(chars.Length)]).ToArray());
    }

    private static byte[] GenerateImage(string code)
    {
        using var bitmap = new Bitmap(120, 40);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.White);

        using var font = new Font("Arial", 20, FontStyle.Bold);
        using var brush = new SolidBrush(Color.DarkBlue);
        graphics.DrawString(code, font, brush, new PointF(10, 5));

        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }
}
