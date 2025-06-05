namespace ApiAlbum.Models.Dtos
{
    public class AlbumDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set ; }
        public string? CoverImageUrl { get; set; }
        public List<SongDto> Songs { get; set; } = new List<SongDto>();
    }
}
