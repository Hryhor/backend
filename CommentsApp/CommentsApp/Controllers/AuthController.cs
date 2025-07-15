using CommentsApp.Interfaces.Auth;
using CommentsApp.Models.DTO;
using CommentsApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace CommentsApp.Controllers
{
    [Route("api/Auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        protected APIResponse _response;
        private readonly ILogger<AuthController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IUserService _userService;

        public AuthController(ILogger<AuthController> logger, UserManager<ApplicationUser> userManager,
             IEmailService emailService, IUserService userService)
        {
            _logger = logger;
            _response = new();
            _userManager = userManager;
            _emailService = emailService;
            _userService = userService;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO requestDTO)
        {
            try
            {
                _logger.LogInformation("success");
                if (requestDTO == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages.Add("Username or password is incorrect");

                    return BadRequest(_response);
                }

                if (!new EmailAddressAttribute().IsValid(requestDTO.Email))
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { "Invalid email format" };

                    return BadRequest(_response);
                }

                var registerResult = await _userService.RegisterAsync(requestDTO);

                if (registerResult.Success == false)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string>() { registerResult.Error };

                    return BadRequest(_response);
                }

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(registerResult.applicationUser);
                var confirmationLink = Url.Action("ConfirmEmail", "Auth", new { userId = registerResult.applicationUser.Id, token = token }, Request.Scheme);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true, // Только для HTTP, не доступно через JavaScript
                    Secure = true,   // Использовать cookie только через HTTPS
                    MaxAge = TimeSpan.FromDays(30), // Срок действия cookie (30 дней)
                    SameSite = SameSiteMode.Strict // Защищает от CSRF атак
                };

                Response.Cookies.Append("refreshToken", registerResult.RefreshToken, cookieOptions);
                await _emailService.SendEmailAsync(registerResult.applicationUser.Email, "Подтверждение почты", $"Перейдите по следующей ссылке для подтверждения: {confirmationLink}");

                _response.Result = new RegisterResponseDTO
                {
                    Success = registerResult.Success,
                    AccessToken = registerResult.AccessToken,
                    RefreshToken = registerResult.RefreshToken,
                    User = registerResult.User,
                };

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                return BadRequest(_response);
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO requestDTO)
        {
            try
            {
                if (requestDTO == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("Data cannot be empty");

                    return BadRequest(_response);
                }

                var registerResult = await _userService.LoginAsync(requestDTO);

                if (registerResult.Success == false)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string>() { registerResult.Error };
                    return BadRequest(_response);
                }

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true, // Только для HTTP, не доступно через JavaScript
                    Secure = true,   // Использовать cookie только через HTTPS
                    MaxAge = TimeSpan.FromDays(30), // Срок действия cookie (30 дней)
                    SameSite = SameSiteMode.Strict // Защищает от CSRF атак
                };

                Response.Cookies.Append("refreshToken", registerResult.RefreshToken, cookieOptions);

                _response.Result = new LoginResponseDTO {
                   AccessToken = registerResult.AccessToken,
                   RefreshToken = registerResult.RefreshToken,
                   User = registerResult.User,
                };
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                return BadRequest(_response);
            }
        }

        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDTO requestDTO)
        {
            try
            {
                var refreshToken = requestDTO.RefreshToken;

                var logoutResult = await _userService.LogoutAsync(refreshToken);

                if (logoutResult.Success == false)
                {
                    Response.Cookies.Delete("refreshToken");
                    _response.IsSuccess = logoutResult.Success;
                    _response.Result = "Failed to log out.";
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                //Response.Cookies.Delete("refreshToken");

                _response.IsSuccess = logoutResult.Success;
                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = "Successfully logged out.";

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                return BadRequest(_response);
            }
        }

        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDTO refreshRequestDTO)
        {
            try
            {
                //var refreshToken = Request.Cookies["refreshToken"];\
                var refreshToken = refreshRequestDTO.refreshToken;
                var userDTO = refreshRequestDTO.userDTO;

                var refreshResult = await _userService.RefreshAsync(refreshRequestDTO);

                if (refreshResult.Success == false)
                {
                    _response.IsSuccess = refreshResult.Success;
                    _response.Result = refreshResult.Error;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                _response.IsSuccess = true;
                _response.Result = new RefreshResponceDTO {
                    RefreshToken = refreshResult.RefreshToken,
                    AccessToken = refreshResult.AccessToken,
                    User = refreshResult.User,
                };
                _response.StatusCode = HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                return BadRequest(_response);
            }
        }

        [HttpGet("confirmemail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var resultConfirmedEmail = await _userService.ConfirmEmailAsync(userId, token);

            if (resultConfirmedEmail.Success == false)
            {
                _response.IsSuccess = resultConfirmedEmail.Success;
                _response.ErrorMessages = new List<string> { resultConfirmedEmail.Message };
                return BadRequest(_response);
            }

            _response.IsSuccess = true;
            _response.Result = resultConfirmedEmail.Message;
            return Ok(_response);
        }
    }
}
