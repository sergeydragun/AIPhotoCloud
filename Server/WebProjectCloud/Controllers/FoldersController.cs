using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using WebProjectCloud.Models;

namespace WebProjectCloud.Controllers
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

        //public IActionResult Index()
        //{
        //    return View(_context.FolderModel.ToList());
        //}

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

        
        //[Route("Folders")]
        //public IActionResult OpenFolder(string folderName)
        //{
        //    return RedirectToRoute("default", new { controller = "Home", action = "Index" });
        //}


    }

}
