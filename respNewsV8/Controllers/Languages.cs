using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using respNewsV8.Models;

namespace respNewsV8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LanguagesController : ControllerBase
    {
        private readonly RespNewContext _sql;
        public LanguagesController(RespNewContext sql)
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
        public List<Language> GetAll(int langId)
        {
            return _sql.Languages.ToList();
        }


        //GET
        [HttpGet("language/{langId}")]
        public List<Language> Get(int langId)
        {
            var lang = langId;
            return _sql.Languages.Where(x=>x.LanguageId==langId).ToList();
        }

        //umumi
        [HttpGet("count")]
        public IActionResult GetLanguageCount()
        {
                // Kategorilerin sayısını almak
                var LanguageCount = _sql.Languages
                    .Select(x => x.LanguageId)  // Kategori ismi
                    .Distinct()                   // Benzersiz kategoriler
                    .Count();                     // Sayma işlemi

                return Ok(new { LanguageCount });  // JSON formatında sayıyı döndürme
            
            
        }

        //GET misal 2dene az dilinde -https://localhost:44314/api/category/count/1
        [HttpGet("count/{langId}")]
        public IActionResult GetLanguageCountByLang(int langId)
        {
            try
            {
                // Kategorilerin sayısını almak
                var LanguageCount = _sql.Languages
                    .Where(x => x.LanguageId == langId) // Dil ID'ye göre filtreleme
                    .Select(x => x.LanguageId)  // Kategori ismi
                    .Distinct()                   // Benzersiz kategoriler
                    .Count();                     // Sayma işlemi

                return Ok(new { LanguageCount });  // JSON formatında sayıyı döndürme
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
            var language = _sql.Languages.SingleOrDefault(x => x.LanguageId == id);

            if (language == null)
            {
                return NotFound();
            }

            return Ok(language);
        }



        //DELETE
        //[Authorize(Roles = "Admin")]

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var a = _sql.Languages.SingleOrDefault(x => x.LanguageId == id);
            _sql.Languages.Remove(a);
            _sql.SaveChanges();
            return NoContent();
        }
    }
}

