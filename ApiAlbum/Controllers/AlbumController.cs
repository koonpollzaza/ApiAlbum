using ApiAlbum.Models;
using ApiAlbum.Models.Dtos;
using ApiAlbum.Models.Request;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;

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

        private AlbumDto MapToDto(Album album)
        {
            return new AlbumDto
            {
                Id = album.Id,
                Name = album.Name,
                Description = album.Description,
                CoverImageUrl = $"/UploadFile/ProfileImg/{album.FileId}/{album.File?.FileName}",
                Songs = album.Songs?.Select(s => new SongDto
                {
                    Id = s.Id,
                    Name = s.Name
                }).ToList() ?? new List<SongDto>()
            };
        }

        [HttpPost("เพิ่มข้อมูล")]
        public ActionResult<AlbumDto> Create([FromForm] RequestCreateAlbum album)
        {
            if (album.File == null || album.File.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            List<Song> songEntitiys = album.SongNames
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => new Song { Name = name })
                .ToList() ?? new List<Song>();

            Album newAlbum = new Album
            {
                Name = album.Name,
                Description = album.Description,
                Songs = songEntitiys
            };

            Models.File file = new Models.File
            {
                FileName = album.File.FileName,
                FilePath = "UploadFile/ProfileImg/"
            };
            file = Models.File.Create(_context, file);

            string uploads = Path.Combine(_hostEnvironment.ContentRootPath, "UploadFile/ProfileImg", file.Id.ToString());
            Directory.CreateDirectory(uploads);

            string filePath = Path.Combine(uploads, album.File.FileName);
            using (Stream fileStream = new FileStream(filePath, FileMode.Create))
            {
                album.File.CopyTo(fileStream);
            }

            newAlbum.FileId = file.Id;
            newAlbum.Create(_context);
            newAlbum.File = file; // for MapToDto

            return Ok(MapToDto(newAlbum));
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
            Album? existingAlbum = _context.Albums
                .Include(a => a.Songs)
                .Include(a => a.File)
                .FirstOrDefault(a => a.Id == album.Id);

            if (existingAlbum == null)
            {
                return NotFound("Album not found");
            }

            existingAlbum.Name = album.Name;
            existingAlbum.Description = album.Description;

            //Update Song
            List<Song> songs = new();

            if (!string.IsNullOrWhiteSpace(album.SongNames))
            {
                var songNames = JsonConvert.DeserializeObject<List<Song>>(album.SongNames);

                foreach (var oldSong in existingAlbum.Songs)
                {
                    oldSong.IsDelete = true;
                    oldSong.UpdateBy = "pon";
                    oldSong.UpdateDate = DateTime.Now;
                }

                foreach (var newSong in songNames)
                {
                    Song song = new Song
                    {
                        Name = newSong.Name,
                        IsDelete = false,
                        CreateBy = "pon",
                        CreateDate = DateTime.Now,
                        UpdateBy = "pon",
                        UpdateDate = DateTime.Now,
                        Album = existingAlbum 
                    };

                    _context.Songs.Add(song);
                }
            }
                    //UpdateFile
            if (album.File != null)
            {
                if (existingAlbum.File == null)
                {
                    ApiAlbum.Models.File newFile = new ApiAlbum.Models.File
                    {
                        FileName = album.File.FileName,
                        CreateBy = "pon",
                        CreateDate = DateTime.Now
                    };

                    ApiAlbum.Models.File createdFile = ApiAlbum.Models.File.Create(_context, newFile);
                    existingAlbum.File = createdFile;
                }
                else
                {
                    existingAlbum.File.FileName = album.File.FileName;
                    existingAlbum.File.UpdateBy = "pon";
                    existingAlbum.File.UpdateDate = DateTime.Now;

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


        [HttpGet("Search/{name}", Name = "SearchAlbumName")]
        public ActionResult SearchAlbumName(string name)
        {
            List<Album> albums = Album.Search(_context, name);

            if (albums.Count == 0)
            {
                return NotFound("ไม่พบอัลบั้มที่ตรงกับชื่อที่ค้นหา");
            }

            foreach (var album in albums)
            {
                album.Songs = album.Songs
                    .Where(song => song.IsDelete == false)
                    .ToList();
            }

            return Ok(albums);
        }


    }
}
