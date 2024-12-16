using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using System.IO;
using respNewsV8.Controllers;
using respNewsV8.Models;
using Microsoft.AspNetCore.RateLimiting;
using respNewsV8.Services;
using YamlDotNet.Core.Tokens;
using YamlDotNet.Core;



namespace respNewsV8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly RespNewContext _sql;
		private readonly UnsplashService _unsplashService;


		public NewsController(RespNewContext sql, UnsplashService unsplashService)
        {
            _sql = sql;
			_unsplashService = unsplashService;

		}
        private int? GetLanguageIdByCode(int langCode,int categoryId)
        {
            var language = _sql.Languages.FirstOrDefault(l => l.LanguageId == langCode);
            var category = _sql.Categories.FirstOrDefault(x => x.CategoryId == categoryId);
            if (language == null)
            {
                Console.WriteLine($"Dil kodu bulunamadı: {langCode}");
                return null; 
            }
            if (category == null)
            {
                Console.WriteLine($"Category kodu bulunamadı: {categoryId}");
                return null;
            }
            return language.LanguageId;
        }






        //umumi
        [HttpGet("count")]
        public IActionResult GetNewsCount()
        {
            // Kategorilerin sayısını almak
            var NewsCount = _sql.News.Where(n => n.NewsStatus == true)

                .Select(x => x.NewsId)  // Kategori ismi
                .Distinct()                   // Benzersiz kategoriler
                .Count();                     // Sayma işlemi

            return Ok(new { NewsCount });  // JSON formatında sayıyı döndürme


        }

        //GET misal 2dene az dilinde -https://localhost:44314/api/category/count/1
        [HttpGet("count/{langId}")]
        public IActionResult GetNewsCountByLang(int langId)
        {
            try
            {
                // Kategorilerin sayısını almak
                var NewsCount = _sql.News
                    .Where(x => x.NewsLangId == langId && x.NewsStatus==true) // Dil ID'ye göre filtreleme
                    .Select(x => x.NewsId)  // Kategori ismi
                    .Distinct()                   // Benzersiz kategoriler
                    .Count();                     // Sayma işlemi

                return Ok(new { NewsCount });  // JSON formatında sayıyı döndürme
            }
            catch (Exception ex)
            {
                // Hata durumunda uygun bir mesaj döndürme
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        //umumi sekiller
        [HttpGet("photos/count")]
        public IActionResult GetNewsPhotosCount()
        {
            // Kategorilerin sayısını almak
            var NewsCount = _sql.NewsPhotos
                .Select(x => x.PhotoId)  // Kategori ismi
                .Distinct()                   // Benzersiz kategoriler
                .Count();                     // Sayma işlemi

            return Ok(new { NewsCount });  // JSON formatında sayıyı döndürme


        }




        [HttpGet("language/{langCode}/{pageNumber}")]
        public IActionResult Get(int langCode,int pageNumber)
        {
            int languageId = langCode;
            int page = pageNumber;
            if (languageId == null)
            {
                return NotFound($"Dil kodu '{langCode}' için bir ID bulunamadı.");
            }

            var newsList = _sql.News
                .Include(n => n.NewsCategory)
                .Include(n => n.NewsLang)
                .Include(n => n.NewsPhotos)
                .Include(n => n.NewsOwner)
                .Where(n => n.NewsStatus == true && n.NewsVisibility == true)
                .Where(n => n.NewsLangId == languageId) 
                .OrderByDescending(x => x.NewsDate)
                .ThenBy(x => x.NewsRating)
                .Select(n => new
                {
                    n.NewsId,
                    n.NewsTitle,
                    n.NewsContetText,
                    n.NewsDate,
                    n.NewsCategoryId,
                    n.NewsCategory,
                    n.NewsLangId,
                    n.NewsLang,
                    n.NewsVisibility,
                    n.NewsStatus,
                    n.NewsRating,
                    n.NewsTags,
                    n.NewsOwner,
                    n.NewsUpdateDate,
                    n.NewsViewCount,
                    n.NewsYoutubeLink,
                    n.NewsPhotos,
                    n.NewsVideos
                }).Skip(page * 3).Take(3).ToList();

            if (!newsList.Any())
            {
                return NotFound($"Dil kodu '{langCode}' için uygun haber bulunamadı.");
            }

            return Ok(newsList);
        }


        //Top 10 news
        [HttpGet("top/language/{langCode}")]
        public IActionResult GetTop(int langCode)
        {
            int languageId = langCode;
            if (languageId == null)
            {
                return NotFound($"Dil kodu '{langCode}' için bir ID bulunamadı.");
            }

            var newsList = _sql.News
                .Include(n => n.NewsCategory)
                .Include(n => n.NewsLang)
                .Include(n => n.NewsPhotos)
                .Include(n => n.NewsOwner)
                .Where(n => n.NewsStatus == true && n.NewsVisibility == true)
                .Where(n => n.NewsLangId == languageId)
                .OrderByDescending(x => x.NewsDate)
                .ThenBy(x => x.NewsRating)
                .Select(n => new
                {
                    n.NewsId,
                    n.NewsTitle,
                    n.NewsContetText,
                    n.NewsDate,
                    n.NewsCategoryId,
                    n.NewsCategory,
                    n.NewsLangId,
                    n.NewsLang,
                    n.NewsRating,
                    n.NewsOwner,
                    n.NewsUpdateDate,
                    n.NewsViewCount,
                    n.NewsYoutubeLink,
                    n.NewsTags,
                    n.NewsPhotos,
                    n.NewsVideos
                }).Take(10).ToList();

            if (!newsList.Any())
            {
                return NotFound($"Dil kodu '{langCode}' için uygun haber bulunamadı.");
            }

            return Ok(newsList);
        }









        [HttpGet("rating/{RatingCode}/{langCode}")]
        public IActionResult GetByRating(int RatingCode,int langCode)
        {
            int RatingId = RatingCode;
            int languageId = langCode;

            if (RatingId == null)
            {
                return NotFound($"Rating kodu '{RatingId}' için bir kod bulunamadı.");
            }
            if (languageId == null)
            {
                return NotFound($"Dil kodu '{languageId}' için bir kod bulunamadı.");
            }
            var newsList = _sql.News
                .Include(n => n.NewsCategory)
                .Include(n => n.NewsLang)
                .Include(n => n.NewsPhotos)
                .Include(n => n.NewsOwner)
                .Where(n => n.NewsStatus == true && n.NewsVisibility == true)
                .Where(n => n.NewsRating == RatingId)
                .Where(n => n.NewsLangId == languageId)
                .OrderByDescending(x => x.NewsDate)
                .Select(n => new
                {
                    n.NewsId,
                    n.NewsTitle,
                    n.NewsContetText,
                    n.NewsDate,
                    n.NewsCategoryId,
                    n.NewsCategory,
                    n.NewsLangId,
                    n.NewsLang,
                    n.NewsVisibility,
                    n.NewsStatus,
                    n.NewsRating,
                    n.NewsOwner,
                    n.NewsUpdateDate,
                    n.NewsViewCount,
                    n.NewsYoutubeLink,
                    n.NewsTags, 
                    n.NewsPhotos,
                    n.NewsVideos
                }).ToList();

            if (!newsList.Any())
            {
                return NotFound($"Dil kodu '{RatingCode}' için uygun haber bulunamadı.");
            }

            return Ok(newsList);
        }



        //Categoryler ucun
        [HttpGet("language/{langCode}/{categoryId}/{pageNumber}")]
        public IActionResult Get(int langCode, int pageNumber,int categoryId)
        {
            int languageId = langCode;
            int category = categoryId;
            int page = pageNumber;
            if (languageId == null)
            {
                return NotFound($"Dil kodu '{langCode}' için bir ID bulunamadı.");
            }

            var newsList = _sql.News
                .Include(n => n.NewsCategory)
                .Include(n => n.NewsLang)
                .Include(n => n.NewsPhotos)
                .Include(n => n.NewsOwner)
                .Where(n => n.NewsStatus == true && n.NewsVisibility == true)
                .Where(n => n.NewsLangId == languageId)
                .Where(n=>n.NewsCategoryId==categoryId)
                .OrderByDescending(x => x.NewsDate)
                .ThenBy(x => x.NewsRating)
                .Select(n => new
                {
                    n.NewsId,
                    n.NewsTitle,
                    n.NewsContetText,
                    n.NewsDate,
                    n.NewsCategoryId,
                    n.NewsCategory,
                    n.NewsLangId,
                    n.NewsLang,
                    n.NewsVisibility,
                    n.NewsStatus,
                    n.NewsRating,
                    n.NewsOwner,
                    n.NewsUpdateDate,
                    n.NewsViewCount,
                    n.NewsYoutubeLink,
                    n.NewsTags,
                    n.NewsPhotos,
                    n.NewsVideos
                }).Skip(page * 3).Take(3).ToList();

            if (!newsList.Any())
            {
                return NotFound($"Dil kodu '{langCode}' için uygun haber bulunamadı.");
            }

            return Ok(newsList);
        }




        ///  GET BY ID
        [HttpGet("id/{id}")]
        public ActionResult GetById(int id)
        {
            // Haber detayını getir
            var news = _sql.News
                .Include(n => n.NewsCategory)
                .Include(n => n.NewsLang)
                .Include(n => n.NewsPhotos)
                .Include(n => n.NewsVideos)
                .Include(n => n.NewsOwner)
                .Include(n => n.NewsAdmin)
                .SingleOrDefault(x => x.NewsId == id && x.NewsStatus == true);

            // Haber bulunamazsa 404 dön
            if (news == null)
            {
                return NotFound(new { Message = "Haber bulunamadı." });
            }

            // Görüntülenme sayısını artır ve kaydet
            news.NewsViewCount++;
            _sql.SaveChanges();

            // İstenen şekle dönüştür
            var result = new
            {
                NewsId = news.NewsId,
                NewsTitle = news.NewsTitle,
                NewsContetText = news.NewsContetText,
                NewsDate = news.NewsDate,
                NewsCategoryId = news.NewsCategoryId,
                NewsCategory = news.NewsCategory.CategoryName,
                NewsLangId = news.NewsLangId,
                NewsLang =  news.NewsLang.LanguageName,
                NewsTags = news.NewsTags,   
                NewsVisibility = news.NewsVisibility,
                NewsRating = news.NewsRating,
                NewsUpdateDate = news.NewsUpdateDate,
                NewsYoutubeLink = news.NewsYoutubeLink,
                NewsPhotos = news.NewsPhotos.Select(p => new { p.PhotoId, p.PhotoUrl }),
                NewsVideos = news.NewsVideos.Select(v => new { v.VideoId, v.VideoUrl }),
                //NewsTags = news.NewsTags.Select(t => new { t.TagId, t.TagName }),
                NewsOwner = new
                {
                    news.NewsOwner.OwnerId,
                    news.NewsOwner.OwnerName
                },
                NewsAdmin = new
                {
                    news.NewsAdmin.UserId,
                    news.NewsAdmin.UserName,
                }
            };

            // JSON formatında dön
            return Ok(result);
        }




        // Adminler ucun GET
        //[Authorize(Roles = "Admin")]
        [HttpGet("admin/{pageNumber}")]
        public List<News> GetForAdmins(DateTime? startDate = null, DateTime? endDate = null,int pageNumber=0)
        {
            int page = pageNumber;

            var query = _sql.News
                .Where(n => n.NewsStatus == true )
                .Include(n => n.NewsCategory)
                .Include(n => n.NewsLang)
                .Include(n => n.NewsCategory)
                .Include(n => n.NewsPhotos)
                .Include(n => n.NewsOwner)
                .Include(n=>n.NewsAdmin)
                .Include(n=>n.NewsVideos)
                .AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(n => n.NewsUpdateDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(n => n.NewsUpdateDate <= endDate.Value);
            }

            return query
                .OrderByDescending(x => x.NewsUpdateDate)
                .ThenByDescending(x => x.NewsDate) 
              .Select(n => new News
              {
                  NewsId = n.NewsId,
                  NewsTitle = n.NewsTitle,  /*+++*/ 
                  NewsContetText = n.NewsContetText,  /*+++*/
                  NewsDate = n.NewsDate,
                  NewsCategoryId = n.NewsCategoryId,
                  NewsCategory = n.NewsCategory,  /*+++*/
                  NewsLangId = n.NewsLangId,
                  NewsLang = n.NewsLang,
                  NewsVisibility = n.NewsVisibility,
                  NewsStatus = n.NewsStatus,
                  NewsRating = n.NewsRating,
                  NewsUpdateDate = n.NewsUpdateDate,
                  NewsViewCount = n.NewsViewCount,
                  NewsYoutubeLink = n.NewsYoutubeLink,
                  NewsPhotos = n.NewsPhotos,
                  NewsVideos = n.NewsVideos,
                  NewsTags = n.NewsTags,
                  NewsOwner =n.NewsOwner, /*+++*/
                  NewsAdmin=n.NewsAdmin

              }).Skip(page*10).Take(10).ToList();
        }



        [HttpGet("slider")]
        public List<News> Slider(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _sql.News.Include(n => n.NewsCategory)
                .Include(n => n.NewsLang)
                .Include(n => n.NewsPhotos)
                .Include(n=>n.NewsOwner)
                .Include(n => n.NewsVideos)
                .AsQueryable();

            // Sadece ratingi 5 olan xeberler
            query = query.Where(n => n.NewsRating == 5 && n.NewsStatus==true);

            // Tarix aralığına göre filtr
            if (startDate.HasValue)
            {
                query = query.Where(n => n.NewsDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(n => n.NewsDate <= endDate.Value);
            }

            // Sıralama: Güncellenme tarixine göre azalan sıralama
            return query

                .OrderByDescending(n => n.NewsDate)
                .Select(n => new News
                {
                    NewsId = n.NewsId,
                    NewsTitle = n.NewsTitle,
                    NewsContetText = n.NewsContetText,
                    NewsDate = n.NewsDate,
                    NewsCategoryId = n.NewsCategoryId,
                    NewsCategory = n.NewsCategory,
                    NewsLangId = n.NewsLangId,
                    NewsLang = n.NewsLang,
                    NewsRating = n.NewsRating,
                    NewsYoutubeLink = n.NewsYoutubeLink,
                    NewsPhotos = n.NewsPhotos,
                    NewsVideos = n.NewsVideos,
                    NewsOwner=n.NewsOwner,
                    NewsTags = n.NewsTags,      
                })
                .ToList();
        }


        //UNSPLASH
        [HttpGet("search")]
        public async Task<IActionResult> SearchPhotos(string query) 
        {
            try
            {
                var photoUrls = await _unsplashService.SearchImageAsync(query);
                return Ok(photoUrls);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }


        // POST
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Post([FromForm] UploadNewsDto uploadNewsDto)
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values.SelectMany(v => v.Errors)
                                                     .Select(e => e.ErrorMessage)
                                                     .ToList();
                return BadRequest(new { errors = errorMessages });
            }

            try
            {
                DateTime newsDate = uploadNewsDto.NewsDate ?? DateTime.Now;

                // Yeni haber oluşturma
                News news = new News
                {
                    NewsTitle = uploadNewsDto.NewsTitle,
                    NewsContetText = uploadNewsDto.NewsContetText,
                    NewsYoutubeLink = uploadNewsDto.NewsYoutubeLink,
                    NewsCategoryId = uploadNewsDto.NewsCategoryId,
                    NewsLangId = uploadNewsDto.NewsLangId,
                    NewsOwnerId = uploadNewsDto.NewsOwnerId,
                    NewsAdminId = uploadNewsDto.NewsAdminId,
                    NewsRating = uploadNewsDto.NewsRating,
                    NewsDate = newsDate,
                    NewsUpdateDate = DateTime.Now,
                    NewsStatus = true,
                    NewsVisibility = true,
                    // Etiketleri virgülle ayırarak kaydediyoruz
                    NewsTags = string.Join(",", uploadNewsDto.NewsTags.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                                     .Distinct())
                };

                _sql.News.Add(news);
                await _sql.SaveChangesAsync();  // Haber kaydını veritabanına ekle

                // Fotoğrafları kaydetme
                if (uploadNewsDto.NewsPhotos?.Any() == true)
                {
                    foreach (var photo in uploadNewsDto.NewsPhotos)
                    {
                        var photoUrl = await SaveFileAsync(photo, "NewsPhotos");

                        NewsPhoto newsPhoto = new NewsPhoto
                        {
                            PhotoUrl = photoUrl,
                            PhotoNewsId = news.NewsId
                        };
                        _sql.NewsPhotos.Add(newsPhoto);
                    }
                }

                // Videoları kaydetme
                if (uploadNewsDto.NewsVideos?.Any() == true)
                {
                    foreach (var video in uploadNewsDto.NewsVideos)
                    {
                        var videoUrl = await SaveFileAsync(video, "NewsVideos");

                        NewsVideo newsVideo = new NewsVideo
                        {
                            VideoUrl = videoUrl,
                            VideoNewsId = news.NewsId
                        };
                        _sql.NewsVideos.Add(newsVideo);
                    }
                }

                await _sql.SaveChangesAsync();  // Veritabanı değişikliklerini kaydet

                return CreatedAtAction(nameof(GetById), new { id = news.NewsId }, news);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server hatası: {ex.Message}");
            }
        }



        private async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            try
            {
                // Dosya uzantısını al
                var fileExtension = Path.GetExtension(file.FileName);

                // Rastgele bir GUID oluştur ve dosya uzantısıyla birleştir
                var fileName = Guid.NewGuid().ToString() + fileExtension;

                // Yükleme klasörünü oluştur
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);
                Directory.CreateDirectory(uploadsFolder);

                // Dosya yolu
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Dosyayı kaydet
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Kaydedilen dosyanın URL'sini döndür
                return $"/{folderName}/{fileName}";
            }
            catch (Exception ex)
            {
                throw new Exception("Dosya kaydedilirken bir hata oluştu: " + ex.Message);
            }
        }




        // DELETE
        //[Authorize(Roles = "Admin")]
        [HttpDelete("id/{id}")]
        public IActionResult Delete(int id)
        {
            var news = _sql.News.SingleOrDefault(x => x.NewsId == id);
            if (news == null)
            {
                return NotFound();
            }

            news.NewsStatus = false;
            news.NewsVisibility = false;
            _sql.SaveChanges();
            return NoContent();
        }

        // EDIT
        // [Authorize(Roles = "Admin")]

        [HttpPut("id/{id}")]
        public async Task<IActionResult> UpdateNews(int id, [FromForm] News news)
        {
            // Haberi bul
            var existingNews = await _sql.News
                .Include(n => n.NewsCategory)
                .Include(n => n.NewsLang)
                .Include(n => n.NewsPhotos)
                .Include(n => n.NewsVideos)
                .Include(n => n.NewsOwner)
                .Include(n => n.NewsAdmin)
                .SingleOrDefaultAsync(x => x.NewsId == id);

            if (existingNews == null)
            {
                return NotFound(new { Message = "Güncellenecek haber bulunamadı." });
            }

            // Haber detaylarını güncelle
            existingNews.NewsTitle = news.NewsTitle;
            existingNews.NewsContetText = news.NewsContetText;
            existingNews.NewsDate = news.NewsDate;
            existingNews.NewsCategoryId = news.NewsCategoryId;
            existingNews.NewsLangId = news.NewsLangId;
            existingNews.NewsVisibility = news.NewsVisibility;
            existingNews.NewsRating = news.NewsRating;
            existingNews.NewsYoutubeLink = news.NewsYoutubeLink;

            // Haber fotoğraflarını güncelle
            if (news.NewsPhotos != null && news.NewsPhotos.Count > 0)
            {
                existingNews.NewsPhotos.Clear();
                foreach (var photo in news.NewsPhotos)
                {
                    if (!string.IsNullOrEmpty(photo.PhotoUrl))
                    {
                        existingNews.NewsPhotos.Add(new NewsPhoto { PhotoUrl = photo.PhotoUrl });
                    }
                }
            }

            // Haber videolarını güncelle
            if (news.NewsVideos != null && news.NewsVideos.Count > 0)
            {
                existingNews.NewsVideos.Clear();
                foreach (var video in news.NewsVideos)
                {
                    if (!string.IsNullOrEmpty(video.VideoUrl))
                    {
                        existingNews.NewsVideos.Add(new NewsVideo { VideoUrl = video.VideoUrl });
                    }
                }
            }

            // Haber sahibi ve admini güncelle
            existingNews.NewsOwnerId = news.NewsOwnerId;
            existingNews.NewsAdminId = news.NewsAdminId;

            // Veritabanına kaydet
            await _sql.SaveChangesAsync();

            return Ok(new { Message = "Haber başarıyla güncellendi." });
        }



        // EDIT (visibility Update )
        //[Authorize(Roles = "Admin")]
        [HttpPut("{id}/visibility")]
        public IActionResult UpdateVisibility(int id, [FromBody] UpdateVisibilityDto visibilityDto)
        {
            var news = _sql.News.SingleOrDefault(x => x.NewsId == id);
            if (news == null)
            {
                return NotFound();
            }

            // Visibility durumunu güncelle
            news.NewsVisibility = visibilityDto.IsVisible;
            news.NewsUpdateDate = DateTime.Now;

            // Değişiklikleri kaydet
            var changes = _sql.SaveChanges();

            // Eğer veri güncellenmişse başarılı yanıt döndür
            if (changes > 0)
            {
                return Ok(new { success = true });
            }
            else
            {
                return BadRequest(new { success = false, message = "Veri güncellenmedi" });
            }
        }








        // DTO for visibility update
        public class UpdateVisibilityDto
        { 
            public bool IsVisible { get; set; }
        }

    }
}