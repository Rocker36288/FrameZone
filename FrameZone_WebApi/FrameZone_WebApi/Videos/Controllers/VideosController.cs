using FrameZone_WebApi.Videos.DTOs;
using FrameZone_WebApi.Videos.Services;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Xabe.FFmpeg;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FrameZone_WebApi.Videos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideosController : ControllerBase
    {
        private readonly VideoServices _videoServices;

        public VideosController(VideoServices videoServices)
        {
            _videoServices = videoServices;
        }


        //api/videos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<VideoCardDto>> GetVideo(int id)
        {
            var dto = await _videoServices.GetVideoCardAsync(id);

            if(dto == null)
            {
                Console.WriteLine("API呼叫成功，但失敗!");
                return NotFound();
            }

            return Ok(dto);
        }

        //api/videos/comment/{id}
        [HttpGet("comment/{id}")]
        public async Task<ActionResult<VideoCardDto>> GetComment(int id)
        {
            var dto = await _videoServices.GetVideoCommentByCommentidAsync(id);

            if (dto == null)
            {
                Console.WriteLine("API呼叫成功，但失敗!");
                return NotFound();
            }

            return Ok(dto);
        }

        //api/videos/channel/{id}
        [HttpGet("channel/{id}")]
        public async Task<ActionResult<VideoCardDto>> Getchannel(int id)
        {
            var dto = await _videoServices.GetChannelbyid(id);

            if (dto == null)
            {
                Console.WriteLine("API呼叫成功，但失敗!");
                return NotFound();
            }

            return Ok(dto);
        }
    }
}

