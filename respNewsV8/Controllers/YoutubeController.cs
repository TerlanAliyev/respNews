using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using respNewsV8.Models;
using respNewsV8.Services;

namespace respNewsV8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YoutubeController : ControllerBase
    {
        private readonly RespNewContext _sql;
        public YoutubeController(RespNewContext sql)
        {
            _sql = sql;
        }


        [HttpGet("videos/{pageNumber}")]
        public IActionResult Get(int pageNumber)
        {
            int page = pageNumber;

            var videos = _sql.Ytvideos
                .Skip(page * 10).Take(10).
                ToList();
            return Ok(videos);
        }


        [HttpGet("video/{id}")]
        public IActionResult GetById(int id)
        {
            var video = _sql.Ytvideos.SingleOrDefault(x=>x.VideoId==id);
            return Ok(video);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Ytvideo ytvideo)
        {
            try
            {
                if (ytvideo.VideoStatus == null)
                {
                    ytvideo.VideoDate = DateTime.Now;
                }
                ytvideo.VideoStatus = true;

                _sql.Ytvideos.Add(ytvideo);
                _sql.SaveChanges();

                return Ok(new { success = true, message = "Video başarıyla eklendi", videoId = ytvideo.VideoId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }



        [HttpPut("edit/{id}")]
        public IActionResult Put(int id,Ytvideo ytvideo)
        {

            var old = _sql.Ytvideos.SingleOrDefault(x=>x.VideoId==id);
            old.VideoTitle=ytvideo.VideoTitle;  
            old.VideoUrl=ytvideo.VideoUrl;  
            old.VideoDate=ytvideo.VideoDate;



            _sql.SaveChanges();
            return Ok();
        }

        [HttpDelete("delete/{id}")]
        public IActionResult Delete(int id)
        {
            var a = _sql.Ytvideos.SingleOrDefault(x=>x.VideoId==id);
            _sql.Ytvideos.Remove(a);
            _sql.SaveChanges();
            return Ok();
        }
        }
}
