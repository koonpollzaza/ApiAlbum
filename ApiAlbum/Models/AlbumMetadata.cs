using System.ComponentModel.DataAnnotations;
using System.Data;
using ApiAlbum.Models.Request;
using Microsoft.EntityFrameworkCore;

namespace ApiAlbum.Models
{
    public class AlbumMetadata
    {

    }
    [MetadataType(typeof(AlbumMetadata))]
    public partial class Album
    {
        public Album Create(ApialbumContext _context)
        {
            IsDelete = false;
            CreateBy = "pon";
            CreateDate = DateTime.Now;
            foreach (Song s in this.Songs)
            {
                if (!string.IsNullOrEmpty(s.Name))
                {
                    s.CreateBy = "pon";
                    s.CreateDate = DateTime.Now;
                    s.IsDelete = false;
                }
            }
            _context.Albums.Add(this);
            _context.SaveChanges();

            return this;
        }
        public static List<Album> GetAll(ApialbumContext _context)
        {
            List<Album> returnThis = _context.Albums
            .Where(q => q.IsDelete != true)
            .Include(a => a.File)
            .Include(a => a.Songs)
            .ToList();
            return returnThis;
        }



        public static Album GetById(ApialbumContext _context, int id)
        {
            Album? album = _context.Albums.Find(id);
            return album ?? new Album();
        }

        public Album Update(ApialbumContext _context)
        {
            IsDelete = false;
            UpdateBy = "pon";
            UpdateDate = DateTime.Now;
            _context.Albums.Update(this);
            _context.SaveChanges();
            return this;
        }
    }
}
