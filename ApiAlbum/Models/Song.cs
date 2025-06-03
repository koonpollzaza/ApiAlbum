using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace ApiAlbum.Models;

[Table("Song")]
public partial class Song
{
    [Key]
    public int Id { get; set; }

    public int AlbumId { get; set; }

    public string Name { get; set; } = null!;

    public string? CreateBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreateDate { get; set; }

    public string? UpdateBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateDate { get; set; }

    public bool? IsDelete { get; set; }

    [ForeignKey("AlbumId")]
    [InverseProperty("Songs")]
    [JsonIgnore]
    public virtual Album Album { get; set; } = null!;
}
