using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageServer.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SQL;
using ImageContracts;

namespace ImageServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImagesController : ControllerBase
    {
        private IImageDB DB;

        public ImagesController(IImageDB db)
        {
            this.DB = db;
        }

        [HttpGet("statistics")]
        public List<ImageRepresentation> GetStatistics()
        {
            return DB.GetStatistics();
        }

        [HttpGet("all")]
        public List<ImageRepresentation> GetAllImages()
        {
            return DB.GetAllImages();
        }

        [HttpGet("{id}")]
        public List<ImageRepresentation> GetImages(string id)
        {
            return DB.GetImages(id);
        }

        [HttpPut]
        public List<ImageRepresentation> RecognizeImage([FromBody] List<ImageRepresentation> Images)
        {
            return DB.RecognizeImage(Images);
        }

        [HttpDelete]
        public string ClearDataBase()
        {
            return DB.ClearDatabase();
        }
    }
}
