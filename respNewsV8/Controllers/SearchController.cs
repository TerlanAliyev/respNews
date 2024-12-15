using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using respNewsV8.Models;
using respNewsV8.Services;

namespace respNewsV8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly RespNewContext _sql;
        public SearchController(RespNewContext sql)
        {
            _sql = sql;

        }
        private int? GetLanguageIdByCode(int langCode)
        {
            var language = _sql.Languages.FirstOrDefault(l => l.LanguageId == langCode);
            if (language == null)
            {
                Console.WriteLine($"Dil kodu bulunamadı: {langCode}");
                return null;
            }
            return language.LanguageId;
        }
        /* -https://localhost:44314/api/search/1/yusif/0*/
        [HttpGet("{langCode}/{query}/{pageNumber?}")] 
        public async Task<IActionResult> GetSearch(int langCode, string query, int pageNumber = 0)
        {
            Console.WriteLine($"LanguageId: {langCode}, Query: {query}, Page: {pageNumber}");

            var results = await _sql.News
                .Include(x => x.NewsCategory)
                .Include(x => x.NewsLang)
                .Where(x =>
                    x.NewsLangId == langCode &&
                    (
                        x.NewsTitle.Contains(query) ||
                        x.NewsContetText.Contains(query) ||
                        x.NewsCategory.CategoryName.Contains(query) ||
                        x.NewsOwner.OwnerName.Contains(query) ||
                        x.NewsTags.Contains(query)
                    )
                )
                .Select(n => new
                {
                    n.NewsId,
                    n.NewsTitle,
                    n.NewsDate,
                    n.NewsCategory.CategoryName,
                    n.NewsLang.LanguageName,
                    n.NewsTags,
                    n.NewsOwner.OwnerName,
                    n.NewsPhotos,
                })
                .Skip(pageNumber * 3).Take(3)
                .ToListAsync();

            if (!results.Any())
            {
                return NotFound("No results found.");
            }

            return Ok(results);
        }


    }
}
