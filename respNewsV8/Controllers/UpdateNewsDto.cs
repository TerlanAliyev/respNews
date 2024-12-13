namespace respNewsV8.Controllers
{
    public class UpdateNewsDto
    {
        public string NewsTitle { get; set; }
        public string NewsContentText { get; set; } // 'ContetText' hatalıydı, 'ContentText' olarak düzelttim.
        public int NewsCategoryId { get; set; }
        public int NewsLangId { get; set; }
        public bool IsVisible { get; set; }

        public int NewsRating { get; set; }
        public string NewsYoutubeLink { get; set; }
        public List<IFormFile>? NewsPhotos { get; set; }
        public List<IFormFile>? NewsVideos { get; set; }
    }
}
