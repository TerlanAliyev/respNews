namespace respNewsV8.Models
{
        public class CategoryDto
        {
            public string? CategoryName { get; set; }
            public IFormFile? CategoryCoverUrl { get; set; }
            public int? CategoryLangId { get; set; }
        }

}
