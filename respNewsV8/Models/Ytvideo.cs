using System;
using System.Collections.Generic;

namespace respNewsV8.Models;

public partial class Ytvideo
{
    public int VideoId { get; set; }

    public string? VideoTitle { get; set; }

    public string? VideoUrl { get; set; }

    public DateTime? VideoDate { get; set; }

    public bool? VideoStatus { get; set; }
}
