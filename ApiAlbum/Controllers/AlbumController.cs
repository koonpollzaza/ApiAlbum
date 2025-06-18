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
        [HttpPost("Create")]
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

            // เตรียมข้อมูลไฟล์
            Models.File file = new Models.File
            {
                FileName = album.File.FileName,
                FilePath = "UploadFile/ProfileImg/" + album.File.FileName
            };
            file = Models.File.Create(_context, file);

            // ❌ ไม่สร้างโฟลเดอร์ย่อยตาม file.Id
            // ✅ ใช้โฟลเดอร์ UploadFile/ProfileImg/ โดยตรง
            string uploads = Path.Combine(_hostEnvironment.ContentRootPath, "UploadFile/ProfileImg");
            Directory.CreateDirectory(uploads); // สร้างโฟลเดอร์นี้ถ้ายังไม่มี

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
        [HttpGet("GetAll_Album")]
            public ActionResult GetAll_Album()
            {
                List<Album> albums = Album.GetAll(_context);
                return Ok(albums);
            }



        [HttpGet("{id}")]
        public ActionResult GetAlbumById(int id)
        {
            var album = _context.Albums
                .Include(a => a.Songs)
                .Include(a => a.File)
                .FirstOrDefault(a => a.Id == id);

            if (album == null)
                return NotFound("Album not found");

            return Ok(album);
        }


        //UPDATE
        [HttpPut("Update")]
        public ActionResult UpdateAlbum([FromForm]RequestUpdateAlbum album)
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
                        if (song.IsDelete == true)
                        {
                            Song existingSong = existingAlbum.Songs.FirstOrDefault(s => s.Id == song.Id);
                            if (existingSong != null)
                            {
                                existingSong.Name = song.Name;
                                existingSong.IsDelete = true;
                                existingSong.UpdateBy = "pon";
                                existingSong.UpdateDate = DateTime.Now;
                            }
                            
                        }
                        else
                        {
                            Song existingSong = existingAlbum.Songs.FirstOrDefault(s => s.Id == song.Id);
                            if (existingSong != null)
                            {
                                existingSong.Name = song.Name;
                                existingSong.IsDelete = false;
                                existingSong.UpdateBy = "pon";
                                existingSong.UpdateDate = DateTime.Now;
                            }
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
                // เตรียมข้อมูลไฟล์ใหม่
                ApiAlbum.Models.File newFile = new ApiAlbum.Models.File
                {
                    FileName = album.File.FileName,
                    FilePath = "UploadFile/ProfileImg/" + album.File.FileName,
                    CreateBy = "pon",
                    CreateDate = DateTime.Now
                };

                // สร้างข้อมูลไฟล์ในฐานข้อมูล
                ApiAlbum.Models.File createdFile = ApiAlbum.Models.File.Create(_context, newFile);

                // อัปโหลดไฟล์ไปยังโฟลเดอร์
                string uploads = Path.Combine(_hostEnvironment.ContentRootPath, "UploadFile/ProfileImg");
                Directory.CreateDirectory(uploads); // สร้างโฟลเดอร์ถ้ายังไม่มี

                string filePath = Path.Combine(uploads, album.File.FileName);
                using (Stream fileStream = new FileStream(filePath, FileMode.Create)) // เขียนทับไฟล์เดิมถ้ามี
                {
                    album.File.CopyTo(fileStream);
                }

                // เชื่อมไฟล์ใหม่กับอัลบั้ม
                existingAlbum.FileId = createdFile.Id;
                existingAlbum.File = createdFile;
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
