using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AuctionPlatformGe.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("upload")]
public class ImagesController : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private readonly IWebHostEnvironment _env;

    public ImagesController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file.Length == 0)
            return BadRequest(new { error = "No file provided" });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(new { error = "Only jpg, png, webp and gif are allowed" });

        // Validate MIME by reading magic bytes — prevents extension spoofing
        if (!await IsValidImageAsync(file))
            return BadRequest(new { error = "File content does not match a supported image format" });

        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadsDir = Path.Combine(webRoot, "uploads");
        Directory.CreateDirectory(uploadsDir);

        var filename = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(uploadsDir, filename);

        await using var stream = System.IO.File.Create(fullPath);
        await file.CopyToAsync(stream);

        return Ok(new { url = $"/uploads/{filename}" });
    }

    private static async Task<bool> IsValidImageAsync(IFormFile file)
    {
        var buf = new byte[12];
        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAsync(buf.AsMemory(0, 12));
        if (read < 4) return false;

        // JPEG: FF D8 FF
        if (buf[0] == 0xFF && buf[1] == 0xD8 && buf[2] == 0xFF) return true;
        // PNG: 89 50 4E 47
        if (buf[0] == 0x89 && buf[1] == 0x50 && buf[2] == 0x4E && buf[3] == 0x47) return true;
        // GIF87a / GIF89a: 47 49 46 38
        if (buf[0] == 0x47 && buf[1] == 0x49 && buf[2] == 0x46 && buf[3] == 0x38) return true;
        // WebP: RIFF????WEBP
        if (read >= 12 &&
            buf[0] == 0x52 && buf[1] == 0x49 && buf[2] == 0x46 && buf[3] == 0x46 &&
            buf[8] == 0x57 && buf[9] == 0x45 && buf[10] == 0x42 && buf[11] == 0x50) return true;

        return false;
    }
}
