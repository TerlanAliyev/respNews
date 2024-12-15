using System.Text.Json.Serialization;

namespace respNewsV8.Controllers
{
    public class UpdateNewsDto
    {
        public int NewsId { get; set; }
        public string? NewsTitle { get; set; }
        public string? NewsContetText { get; set; }
        public DateTime? NewsDate { get; set; }
        public int NewsCategoryId { get; set; }
        public string? NewsCategory { get; set; } // Kategori adı
        public int NewsLangId { get; set; }
        public string? NewsLang { get; set; } // Dil adı
        public string? NewsTags { get; set; } // Etiketler
        public bool NewsVisibility { get; set; }
        public int NewsRating { get; set; }
        public DateTime NewsUpdateDate { get; set; }
        public string? NewsYoutubeLink { get; set; }

        // Fotoğraflar ve Videolar
        public List<NewsPhotoDto>? NewsPhotos { get; set; }
        public List<NewsVideoDto>? NewsVideos { get; set; }

        // Sahip ve Admin ID
        public int? NewsOwnerId { get; set; }
        public int? NewsAdminId { get; set; }
    }

    public class NewsPhotoDto
    {
        public string PhotoUrl { get; set; }
    }

    public class NewsVideoDto
    {
        public string VideoUrl { get; set; }
    }

}
