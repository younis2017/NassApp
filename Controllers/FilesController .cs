using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/files")]
public class FilesController: ControllerBase
    {
    private readonly IWebHostEnvironment _env;

    public FilesController (IWebHostEnvironment env)
        {
        _env = env;
        }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload (IFormFile file)
        {
        if (file == null || file.Length == 0)
            return BadRequest("No file");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{extension}";

        var uploadFolder = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadFolder);

        var path = Path.Combine(uploadFolder, fileName);

        using var fs = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(fs);

        return Ok(new
            {
            url = $"/uploads/{fileName}"
            });
        }
    }
