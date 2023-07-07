using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace WebProjectCloud.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PhotosController : Controller
    {
        [Route("{filename}")]
        [HttpGet]
        public async Task<IActionResult> Get(string filename)
        {
            string file_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Photos", filename);
            string file_type;
            var forfile_type = new FileExtensionContentTypeProvider().TryGetContentType(file_path, out file_type);

            return PhysicalFile(file_path, file_type);
        }
    }
}
