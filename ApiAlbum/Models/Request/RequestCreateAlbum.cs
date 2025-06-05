namespace ApiAlbum.Models.Request
{
        public partial class RequestCreateAlbum
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public List<string> SongNames { get; set; }
            public IFormFile? File { get; set; }

    }
}
