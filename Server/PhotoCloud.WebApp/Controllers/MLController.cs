using Microsoft.AspNetCore.Mvc;
using PhotoCloud.Models;

namespace PhotoCloud.Controllers
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
        public Task Post()
        {
           return _detection.SetInDbAsync();
        }

    }
}
