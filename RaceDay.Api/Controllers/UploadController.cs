using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly BlobStorageService _blob;
        public UploadController(BlobStorageService blob)
        {
            _blob = blob;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file selected.");
            var url = await _blob.UploadAsync(file, "uploads");
            return Ok(new { imageUrl = url });
        }
    }
}