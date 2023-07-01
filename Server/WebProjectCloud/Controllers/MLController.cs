using Microsoft.AspNetCore.Mvc;
using WebProjectCloud.Models;

namespace WebProjectCloud.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MLController : Controller
    {
        private IObjectDetection _detection;
        public MLController(IObjectDetection detection) 
        {
            _detection = detection;
        }

        [Route("CheckPhotos")]
        [HttpGet]
        public void Post()
        {
            _detection.SetInDb();
        }

    }
}
