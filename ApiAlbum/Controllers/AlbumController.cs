using ApiAlbum.Models;
using ApiAlbum.Models.Request;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpPut]
        public ActionResult UpdateAlbum([FromForm] RequestUpdateAlbum album)
        {
            var existingAlbum = _context.Albums
                .Include(a => a.Songs)
                .Include(a => a.File)
                .FirstOrDefault(a => a.Id == album.Id);

            if (existingAlbum == null)
                return NotFound("Album not found");

            existingAlbum.Name = album.Name;
            existingAlbum.Description = album.Description;

            if (album.SongNames != null)
            {
                // ลบเพลงเก่าใน DB ก่อน
                if (existingAlbum.Songs.Any())
                {
                    _context.Songs.RemoveRange(existingAlbum.Songs);
                }
                existingAlbum.Songs = album.SongNames.Select(songName => new Song
                {
                    Name = songName,
                    IsDelete = false,
                    CreateBy = "pon",
                    CreateDate = DateTime.Now
                }).ToList();
            }

            // อัปเดตไฟล์ (ถ้ามี)
            if (album.File != null)
            {
                //if (existingAlbum.File == null)
                //{
                //    var newFile = ApiAlbum.Models.File.Create(_context, album.File);
                //    existingAlbum.File = newFile;
                //}
                // อัปเดตไฟล์ (ถ้ามี)
                if (album.File != null && existingAlbum.File != null)
                {
                    existingAlbum.File.FileName = album.File.FileName;
                    ApiAlbum.Models.File.Update(_context, existingAlbum.File);
                }

            }

            existingAlbum.Update(_context);

            return Ok(existingAlbum);
        }


        [HttpDelete("{id}")]
        public ActionResult DeleteAlbum(int id)
        {
            Album album = Album.Delete(_context, id);
            return Ok(album);
        }
    }
}
