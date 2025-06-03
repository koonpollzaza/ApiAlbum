using ApiAlbum.Models;
using ApiAlbum.Models.Request;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ApiAlbum.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AlbumController : ControllerBase
    {
        private ApialbumContext _context = new ApialbumContext();
        private IHostEnvironment _hostEnvironment;
        private readonly ILogger<AlbumController> _logger;

        public AlbumController(ILogger<AlbumController> logger, IHostEnvironment environment)
        {
            _logger = logger;
            _hostEnvironment = environment;
        }
        [HttpPost("เพิ่มข้อมูล")]
        public ActionResult<Album> Create ([FromForm] RequestCreateAlbum album)
        {
            if (album.File == null || album.File.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            Album newAlbum = new Album
            {
                Name = album.Name,
                Description = album.Description,
                Songs = album.SongNames?.Select(name => new Song
                {
                    Name = name
                }).ToList() ?? new List<Song>()
            };

            Models.File file = new Models.File
            {
                FileName = album.File.FileName,
                FilePath = "UploadFile/ProfileImg/"
            };
                file = Models.File.Create(_context, file);

                string uploads = Path.Combine(_hostEnvironment.ContentRootPath, "UploadFile/ProfileImg/" + file.Id);
                Directory.CreateDirectory(uploads);

                string filePath = Path.Combine(uploads, album.File.FileName);
                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    album.File.CopyTo(fileStream);
                }

            newAlbum.FileId = file.Id;
            newAlbum.Create(_context);

            return Ok(newAlbum);
        }
        [HttpGet("GetAll_Album ดูข้อมูลทั้งหมด")]
        public ActionResult GetAll_Album()
        {
            List<Album> albums = Album.GetAll(_context);
            return Ok(albums);
        }

        //[HttpPut]
        //public ActionResult<Album> UpdateAlbum (Album album)
        //{
        //    Album updateAlbum = Album.GetById(_context, album.Id){ 
        //    }
        //}
    }
}
