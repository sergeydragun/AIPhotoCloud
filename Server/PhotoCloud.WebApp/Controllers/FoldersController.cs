using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoCloud.Models;

namespace PhotoCloud.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class FoldersController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly IWebHostEnvironment _enviroment;
        public FoldersController(ApplicationContext context, IWebHostEnvironment enviroment)
        {
            _context = context;
            _enviroment = enviroment;
        }

        [HttpGet]
        public async Task<JsonResult> Get()
        {
            return new JsonResult(await _context.FolderModel.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Post(FolderModel folder)
        {
            _context.FolderModel.Add(folder);
            await _context.SaveChangesAsync();

            return new JsonResult(new { message = "Added succesfuly" });
        }

    }

}
