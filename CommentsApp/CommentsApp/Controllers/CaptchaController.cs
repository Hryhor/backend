using CommentsApp.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CommentsApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CaptchaController : ControllerBase
    {
        private readonly CaptchaService _captchaService;

        public CaptchaController(CaptchaService captchaService)
        {
            _captchaService = captchaService;
        }

        [HttpGet("generate")]
        public IActionResult Generate()
        {
            var (image, id) = _captchaService.GenerateCaptcha();
            Response.Headers.Add("Captcha-Id", id);
            Response.Headers.Add("Access-Control-Expose-Headers", "Captcha-Id");
            return File(image, "image/png");
        }

        [HttpPost("validate")]
        public IActionResult Validate([FromBody] CaptchaCheckDTO dto)
        {
            var isValid = _captchaService.ValidateCaptcha(dto.CaptchaId, dto.UserInput);
            return Ok(new { isValid });
        }
    }
}


