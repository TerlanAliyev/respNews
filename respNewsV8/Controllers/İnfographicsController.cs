using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using respNewsV8.Models;
using System.IO;

namespace respNewsV8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InfographicsController : ControllerBase
    {
        private readonly RespNewContext _sql;

        public InfographicsController(RespNewContext sql)
        {
            _sql = sql;
        }

        [HttpGet("{pageNumber}")]
        public IActionResult Get(int pageNumber)
        {
            int page = pageNumber;

            var infs = _sql.İnfographics
                .OrderByDescending(x=>x.InfPostDate)
                .Skip(page * 10).Take(10).ToList();
            return Ok(infs);
        }

        //umumi
        [HttpGet("count")]
        public IActionResult GetInfCount()
        {
            var InfCount = _sql.İnfographics
                .Select(x => x.InfId)  // Kategori ismi
                .Distinct()                   // Benzersiz kategoriler
                .Count();                     // Sayma işlemi

            return Ok(new { InfCount });  // JSON formatında sayıyı döndürme


        }

        [HttpGet("id/{id}")]
        public IActionResult GetbyId(int id)
        {
            var infs = _sql.İnfographics.SingleOrDefault(x => x.InfId == id);
            return Ok(infs);
        }


        //[Authorize(Roles = "Admin")]/
        [HttpDelete("delete/{id}")]
        public IActionResult Delete(int id)
        {
            var infs = _sql.İnfographics.SingleOrDefault(x => x.InfId == id);
            _sql.Remove(infs);
            _sql.SaveChanges();
            return Ok(infs);
        }


        //[Authorize(Roles = "Admin")]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadInfographic([FromForm] InfographicDTO infographicDTO)
        {
            if (infographicDTO.InfPhoto == null || infographicDTO.InfPhoto.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var fileExtension = Path.GetExtension(infographicDTO.InfPhoto.FileName);
            var fileName = Guid.NewGuid().ToString() + fileExtension; // Rastgele isim oluştur
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "InfPhotos", fileName);

            try
            {
                // Dosyayı kaydetme
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await infographicDTO.InfPhoto.CopyToAsync(stream);
                }

                // Veritabanına kaydetme
                var infographic = new İnfographic
                {
                    InfName = infographicDTO.InfName,
                    InfPhoto = $"/InfPhotos/{fileName}", // URL'yi kaydet
                    InfPostDate = DateTime.Now
                };

                _sql.İnfographics.Add(infographic);  // Infographic nesnesini ekle
                await _sql.SaveChangesAsync();  // Değişiklikleri kaydet

                return Ok(new { message = "File uploaded and saved to database", fileUrl = infographic.InfPhoto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal Server Error", error = ex.Message });
            }
        }



        [HttpPut("edit/{id}")]
        public IActionResult UpdateNews(int id, [FromBody] InfUpdateDto InfUpdateDto)
        {
            // Haberi bul
            var existingNews = _sql.İnfographics
                .SingleOrDefault(x => x.InfId == id);

            if (existingNews == null)
            {
                return NotFound(new { Message = "Güncellenecek haber bulunamadı." });
            }

            // Haber detaylarını güncelle
            existingNews.InfName = InfUpdateDto.InfName;
            existingNews.InfPostDate = InfUpdateDto.InfPostDate;
            existingNews.InfPostDate=InfUpdateDto.InfPostDate;

            
            // Veritabanına kaydet
            _sql.SaveChanges();

            return Ok(new { Message = "Haber başarıyla güncellendi." });
        }







    }
}
