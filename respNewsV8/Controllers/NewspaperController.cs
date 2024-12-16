using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using respNewsV8.Models;

namespace respNewsV8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewspaperController : ControllerBase
    {
        private readonly RespNewContext _sql;

        public NewspaperController(RespNewContext sql)
        {
            _sql = sql;
        }

        // GET BY ID
        [HttpGet("{id}")]
        public IActionResult GetNewspaperById(int id)
        {
            var newspaper = _sql.Newspapers.Find(id);
            if (newspaper == null)
                return NotFound("Gazete bulunamadı.");

            return Ok(newspaper);
        }

        // GET ALL
        [HttpGet("all/{pageNumber}")]
        public IActionResult GetAllNewspapers(int pageNumber = 0)
        {
            int page = pageNumber;

            var newspapers = _sql.Newspapers
                .OrderByDescending(x=>x.NewspaperDate)
                .Skip(page * 5).Take(5)
                .ToList();
            return Ok(newspapers);
        }

        [HttpPut("{id}/visibility")]
        public IActionResult UpdateVisibility(int id, [FromBody] UpdatePdfVisibilityDto dto)
        {
            // İlgili kategoriyi ID'ye göre sorgula
            var paper = _sql.Newspapers.SingleOrDefault(x => x.NewspaperId == id);

            if (paper == null)
            {
                return NotFound(new { success = false, message = "Pdf bulunamadı" });
            }

            // Gelen DTO'daki görünürlük durumunu kategoriye ata
            paper.NewspaperStatus = dto.IsVisible;

            // Veritabanı değişikliklerini kaydet
            var changes = _sql.SaveChanges();

            // Güncelleme başarılıysa olumlu bir yanıt döndür
            if (changes > 0)
            {
                return Ok(new { success = true, message = "Pdf durumu güncellendi" });
            }

            // Güncelleme başarısızsa hata mesajı döndür
            return BadRequest(new { success = false, message = "Pdf durumu güncellenemedi" });
        }


        // DTO for visibility update
        public class UpdatePdfVisibilityDto
        {
            public bool IsVisible { get; set; }
        }


        //umumi
        [HttpGet("count")]
        public IActionResult GetNewsPaperCount()
        {
            // Kategorilerin sayısını almak
            var NewsPaperCount = _sql.Newspapers
                .Select(x => x.NewspaperId)  // Kategori ismi
                .Distinct()                   // Benzersiz kategoriler
                .Count();                     // Sayma işlemi

            return Ok(new { NewsPaperCount });  // JSON formatında sayıyı döndürme


        }


       


        // GET LAST
        [HttpGet("last")]
        public IActionResult GetLastNewspapers()
        {
            var Lastnewspapers = _sql.Newspapers.OrderByDescending(x=>x.NewspaperDate).FirstOrDefault();
            return Ok(Lastnewspapers);
        }


        // GET PDF URL
        [HttpGet("pdf/{id}")]
        public IActionResult GetPdf(int id)
        {
            var newspaper = _sql.Newspapers.Find(id);
            if (newspaper == null)
                return NotFound("Gazete bulunamadı.");

            if (string.IsNullOrEmpty(newspaper.NewspaperPdfUrl))
                return NotFound("PDF URL'si bulunamadı.");

            return Redirect(newspaper.NewspaperPdfUrl);
        }


       


        [HttpDelete("id/{id}")]
        public IActionResult DeleteNewsPaper(int id)
        {
            var a = _sql.Newspapers.SingleOrDefault(x => x.NewspaperId == id);
            _sql.Newspapers.Remove(a);
            _sql.SaveChanges();
            return Ok();
        }

        [HttpPut("edit/{id}")]
        public IActionResult UpdateNews(int id, [FromForm] Newspaper newspaper)
        {
            // Haberi bul
            var existingNews = _sql.Newspapers
                .SingleOrDefault(x => x.NewspaperId == id);

            if (existingNews == null)
            {
                return NotFound(new { Message = "Güncellenecek haber bulunamadı." });
            }

            // Haber detaylarını güncelle
            existingNews.NewspaperTitle = newspaper.NewspaperTitle;
            existingNews.NewspaperLinkFlip = newspaper.NewspaperLinkFlip;
            existingNews.NewspaperDate = newspaper.NewspaperDate;
            existingNews.NewspaperStatus = newspaper.NewspaperStatus;
            existingNews.NewspaperPrice = newspaper.NewspaperPrice;

            // Kapak fotoğrafını kaydet
            if (newspaper.NewspaperCoverFile != null)
            {
                var coversFolderPath = Path.Combine("wwwroot", "NewspaperCovers");
                if (!Directory.Exists(coversFolderPath))
                {
                    Directory.CreateDirectory(coversFolderPath);
                }

                var coverFileName = $"{Guid.NewGuid()}_{Path.GetFileName(newspaper.NewspaperCoverFile.FileName)}";
                var coverFilePath = Path.Combine(coversFolderPath, coverFileName);

                using (var stream = new FileStream(coverFilePath, FileMode.Create))
                {
                    newspaper.NewspaperCoverFile.CopyTo(stream);
                }

                existingNews.NewspaperCoverUrl = $"/NewspaperCovers/{coverFileName}";
            }

            // PDF dosyasını kaydet
            if (newspaper.NewspaperPdfFile != null)
            {
                var pdfFolderPath = Path.Combine("wwwroot", "NewspaperPDF");
                if (!Directory.Exists(pdfFolderPath))
                {
                    Directory.CreateDirectory(pdfFolderPath);
                }

                var pdfFileName = $"{Guid.NewGuid()}_{Path.GetFileName(newspaper.NewspaperPdfFile.FileName)}";
                var pdfFilePath = Path.Combine(pdfFolderPath, pdfFileName);

                using (var stream = new FileStream(pdfFilePath, FileMode.Create))
                {
                    newspaper.NewspaperPdfFile.CopyTo(stream);
                }

                existingNews.NewspaperPdfUrl = $"/NewspaperPDF/{pdfFileName}";
            }

            // Veritabanına kaydet
            _sql.SaveChanges();

            return Ok(new { Message = "Gazete başarıyla güncellendi." });
        }



        // POST
        [HttpPost]
        public async Task<IActionResult> CreateNewspaper([FromForm] newspaperDto newspaperDto)
        {
            if (newspaperDto == null)
                return BadRequest("Geçersiz gazete verisi.");

            var newspaper = new Newspaper
            {
                NewspaperTitle = newspaperDto.NewspaperTitle,
                NewspaperLinkFlip = newspaperDto.NewspaperLinkFlip,
                NewspaperStatus = true,
                NewspaperPrice = newspaperDto.NewspaperPrice,
                NewspaperDate = newspaperDto.NewspaperDate
            };

            // Cover upload
            if (newspaperDto.NewspaperCoverUrl != null && newspaperDto.NewspaperCoverUrl.Length > 0)
            {
                var coverFileExtension = Path.GetExtension(newspaperDto.NewspaperCoverUrl.FileName);
                var coverFileName = Guid.NewGuid().ToString() + coverFileExtension; // Rastgele isim oluştur
                var coverUploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "NewspaperCovers");

                if (!Directory.Exists(coverUploadsFolder))
                {
                    Directory.CreateDirectory(coverUploadsFolder);
                }

                var coverFilePath = Path.Combine(coverUploadsFolder, coverFileName);

                using (var stream = new FileStream(coverFilePath, FileMode.Create))
                {
                    await newspaperDto.NewspaperCoverUrl.CopyToAsync(stream);
                }

                newspaper.NewspaperCoverUrl = $"/NewspaperCovers/{coverFileName}";
            }
            else
            {
                return BadRequest("Kapak dosyası yüklenmedi.");
            }

            // PDF document upload
            if (newspaperDto.NewspaperPdfFile != null && newspaperDto.NewspaperPdfFile.Length > 0)
            {
                var pdfFileExtension = Path.GetExtension(newspaperDto.NewspaperPdfFile.FileName);
                var pdfFileName = Guid.NewGuid().ToString() + pdfFileExtension; // Rastgele isim oluştur
                var pdfUploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "NewspaperPDF");

                if (!Directory.Exists(pdfUploadsFolder))
                {
                    Directory.CreateDirectory(pdfUploadsFolder);
                }

                var pdfFilePath = Path.Combine(pdfUploadsFolder, pdfFileName);

                using (var stream = new FileStream(pdfFilePath, FileMode.Create))
                {
                    await newspaperDto.NewspaperPdfFile.CopyToAsync(stream);
                }

                newspaper.NewspaperPdfUrl = $"/NewspaperPDF/{pdfFileName}";
            }
            else
            {
                return BadRequest("PDF dosyası yüklenmedi.");
            }

            try
            {
                _sql.Newspapers.Add(newspaper);
                await _sql.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Sunucu hatası: {ex.Message}");
            }

            return CreatedAtAction(nameof(GetNewspaperById), new { id = newspaper.NewspaperId }, newspaper);
        }


    }
}

