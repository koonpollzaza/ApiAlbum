using System.ComponentModel.DataAnnotations;

namespace ApiAlbum.Models
{
    public class FileMetadata
    {
    }
    [MetadataType(typeof(FileMetadata))]
    public partial class File
    {
        public static File Create(ApialbumContext _context, File file)
        {
            file.CreateBy = "pon";
            file.CreateDate = DateTime.Now;
            file.IsDelete = false;

            _context.Files.Add(file);
            _context.SaveChanges();

            return file;
        }
        public static File Update(ApialbumContext _context, File file)
        {
            file.CreateBy = "pon";
            file.CreateDate = DateTime.Now;
            file.UpdateBy = "pon";
            file.UpdateDate = DateTime.Now;
            file.IsDelete = false;

            _context.Files.Update(file);
            _context.SaveChanges();

            return file;
        }
        public static File Delete(ApialbumContext _context, File file)
        {
            file.UpdateBy = "pon";
            file.UpdateDate = DateTime.Now;
            file.IsDelete = true;

            _context.Files.Update(file); 
            _context.SaveChanges();       

            return file;
        }

    }
}
