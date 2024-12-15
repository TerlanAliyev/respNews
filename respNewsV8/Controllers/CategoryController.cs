using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using respNewsV8.Models;
using static respNewsV8.Controllers.NewsController;

namespace respNewsV8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly RespNewContext _sql;
        public CategoryController(RespNewContext sql)
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

        //GET
        [HttpGet("all")]
        public List<Category> GetAll(int langId)
        {
            return _sql.Categories.ToList();
        }


        //GET
        [HttpGet("language/{langId}")]
        public List<Category> Get(int langId)
        {
            var lang = langId;
            return _sql.Categories.Where(x=>x.CategoryLangId==langId).ToList();
        }

        //umumi
        [HttpGet("count")]
        public IActionResult GetCategoryCount()
        {
                // Kategorilerin sayısını almak
                var categoryCount = _sql.Categories
                    .Select(x => x.CategoryName)  // Kategori ismi
                    .Distinct()                   // Benzersiz kategoriler
                    .Count();                     // Sayma işlemi

                return Ok(new { categoryCount });  // JSON formatında sayıyı döndürme
            
            
        }

        //GET misal 2dene az dilinde -https://localhost:44314/api/category/count/1
        [HttpGet("count/{langId}")]
        public IActionResult GetCategoryCountByLang(int langId)
        {
            try
            {
                // Kategorilerin sayısını almak
                var categoryCount = _sql.Categories
                    .Where(x => x.CategoryLangId == langId) // Dil ID'ye göre filtreleme
                    .Select(x => x.CategoryName)  // Kategori ismi
                    .Distinct()                   // Benzersiz kategoriler
                    .Count();                     // Sayma işlemi

                return Ok(new { categoryCount });  // JSON formatında sayıyı döndürme
            }
            catch (Exception ex)
            {
                // Hata durumunda uygun bir mesaj döndürme
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }


        // GET by ID 
        [HttpGet("{id}")]
        public ActionResult<Category> GetById(int id)
        {
            var category = _sql.Categories.SingleOrDefault(x => x.CategoryId == id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/visibility")]
        public IActionResult UpdateVisibility(int id, [FromBody] UpdateCategoryVisibilityDto dto)
        {
            // İlgili kategoriyi ID'ye göre sorgula
            var category = _sql.Categories.SingleOrDefault(x => x.CategoryId == id);

            if (category == null)
            {
                return NotFound(new { success = false, message = "Kategori bulunamadı" });
            }

            // Gelen DTO'daki görünürlük durumunu kategoriye ata
            category.CategoryStatus = dto.IsVisible;

            // Veritabanı değişikliklerini kaydet
            var changes = _sql.SaveChanges();

            // Güncelleme başarılıysa olumlu bir yanıt döndür
            if (changes > 0)
            {
                return Ok(new { success = true, message = "Kategori durumu güncellendi" });
            }

            // Güncelleme başarısızsa hata mesajı döndür
            return BadRequest(new { success = false, message = "Kategori durumu güncellenemedi" });
        }







        // POST 
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Post([FromForm] CategoryDto categoryDto)
        {
            if (categoryDto == null || categoryDto.CategoryCoverUrl == null)
            {
                return BadRequest("Geçersiz kategori veya dosya verisi.");
            }

            // Dosya için kaydedilecek yolu belirliyoruz.
            var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "CategoryCovers");

            // Eğer CategoryCovers klasörü yoksa, oluşturulmasını sağlıyoruz.
            if (!Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory);
            }

            // Benzersiz bir dosya adı oluşturuyoruz (Guid ile)
            var fileExtension = Path.GetExtension(categoryDto.CategoryCoverUrl.FileName);
            var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;

            // Dosyanın kaydedileceği tam yol
            var filePath = Path.Combine(uploadDirectory, uniqueFileName);

            // Dosyayı kaydediyoruz.
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await categoryDto.CategoryCoverUrl.CopyToAsync(stream);
            }

            // Kategori nesnesini oluşturuyoruz.
            var category = new Category
            {
                CategoryName = categoryDto.CategoryName,
                CategoryCoverUrl = $"/CategoryCovers/{uniqueFileName}", // Web üzerinden erişilecek URL
                CategoryLangId = categoryDto.CategoryLangId
            };

            // Kategoriyi veritabanına ekliyoruz.
            _sql.Categories.Add(category);
            _sql.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, category);
        }


        //DELETE
        [Authorize(Roles = "Admin")]

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var a = _sql.Categories.SingleOrDefault(x => x.CategoryId == id);
            _sql.Categories.Remove(a);
            _sql.SaveChanges();
            return NoContent();
        }
    }
}

