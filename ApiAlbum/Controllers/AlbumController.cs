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


//CREATE
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






//ดูข้อมูลงับ เบ๊บ
            [HttpGet("GetAll_Album ดูข้อมูลทั้งหมด")]
            public ActionResult GetAll_Album()
            {
                List<Album> albums = Album.GetAll(_context);
                return Ok(albums);
            }






//UPDATE
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

            if (!string.IsNullOrWhiteSpace(album.SongNames))
            {
                List<Song> inSongs = JsonConvert.DeserializeObject<List<Song>>(album.SongNames);

                // ดึง Id เพลงที่มีอยู่ในรายการใหม่ (คือเพลงที่ไม่ถูกลบ)
                var Ids = inSongs
                    .Where(s => s.Id != 0)
                    .Select(s => s.Id)
                    .ToHashSet();

                // UPDATE เพลงเก่า หรือเพิ่มเพลงใหม่
                foreach (Song song in inSongs)
                {
                    if (song.Id != 0)
                    {
                        // EDIT เพลงเก่า
                        Song existingSong = existingAlbum.Songs.FirstOrDefault(s => s.Id == song.Id);
                        if (existingSong != null)
                        {
                            existingSong.Name = song.Name;
                            existingSong.IsDelete = false;
                            existingSong.UpdateBy = "pon";
                            existingSong.UpdateDate = DateTime.Now;
                        }
                    }
                    else
                    {
                        Song newSong = new Song
                        {
                            Name = song.Name,
                            Album = existingAlbum,
                            IsDelete = false,
                            CreateBy = "pon",
                            CreateDate = DateTime.Now,
                            UpdateBy = "pon",
                            UpdateDate = DateTime.Now
                        };
                        _context.Songs.Add(newSong);
                    }
                }
            }
// Update File
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







//DELETE
        [HttpDelete("{id}")]
            public ActionResult DeleteAlbum(int id)
            {
                Album album = Album.Delete(_context, id);
                return Ok(album);
            }







//SEARCH NAME
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
