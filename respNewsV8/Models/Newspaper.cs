using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace respNewsV8.Models;

public partial class Newspaper
{
    public int NewspaperId { get; set; }

    public string? NewspaperTitle { get; set; }

    public string? NewspaperLinkFlip { get; set; }

    public DateTime? NewspaperDate { get; set; }

    public bool? NewspaperStatus { get; set; }

    public string? NewspaperPrice { get; set; }

    public string? NewspaperCoverUrl { get; set; }

    public string? NewspaperPdfUrl { get; set; }

    // Formdan gelen dosyalar için IFormFile alanları
    [NotMapped] // Bu alanlar EF Core tarafından haritalanmaz
    public IFormFile? NewspaperCoverFile { get; set; }

    [NotMapped]
    public IFormFile? NewspaperPdfFile { get; set; }
}
