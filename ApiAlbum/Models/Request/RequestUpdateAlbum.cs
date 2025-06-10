using System.ComponentModel.DataAnnotations;
using ApiAlbum.Models.Dtos;

namespace ApiAlbum.Models.Request
{
    public partial class RequestUpdateAlbum
    {
        [Required]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile? File { get; set; }
        public string SongNames { get; set; }

    }
}
