using AutoMapper;
using CommentsApp.Interfaces.Auth;
using CommentsApp.Models.DTO;
using CommentsApp.Models;
using CommentsApp.Repository.IRepository;
using Microsoft.AspNetCore.Identity;

namespace CommentsApp.Services
{
    public class UserService : IUserService
    {
        protected APIResponse _response;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IPasswordService _passwordService;
        private readonly IAuthRepository _authRepository;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;


        public UserService(IPasswordService passwordService, IAuthRepository authRepository,
           UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
           IMapper mapper, IEmailService emailService, ITokenService tokenService)
        {
            _response = new();
            _userManager = userManager;
            _passwordService = passwordService;
            _authRepository = authRepository;
            _roleManager = roleManager;
            _mapper = mapper;
            _emailService = emailService;
            _tokenService = tokenService;
        }

        public async Task<RegisterResultDTO> RegisterAsync(RegisterRequestDTO requestDTO)
        {
            try
            {
                var applicationUser = new ApplicationUser()
                {
                    Email = requestDTO.Email,
                    NormalizedEmail = requestDTO.Email.ToUpper(),
                    Name = requestDTO.Name,
                    UserName = requestDTO.UserName,
                };

                var createdUser = await _authRepository.CreateUserAsync(applicationUser, requestDTO.Password); 

                if (!createdUser)
                {
                    return new RegisterResultDTO()
                    {
                        Success = false,
                        Error = "User creation failed",
                        AccessToken = null,
                        RefreshToken = null,
                        applicationUser = null,
                        User = null,
                    };
                }

                if (!await _authRepository.RoleExistsAsync("user"))
                {
                    await _authRepository.CreateRoleAsync("user");
                }

                if (!await _authRepository.RoleExistsAsync("admin"))
                {
                    await _authRepository.CreateRoleAsync("admin");
                }

                if (requestDTO.Email == "hryhorenko.illia@gmail.com")
                {
                    await _authRepository.AddUserToRoleAsync(applicationUser, "admin");
                }
                else
                {
                    await _authRepository.AddUserToRoleAsync(applicationUser, "user");
                }

                var userToReturn = await _authRepository.GetUserByUserNameAsync(requestDTO.UserName);

                if (userToReturn == null)
                {
                    return new RegisterResultDTO()
                    {
                        Success = true,
                        Error = "User creation failed",
                        AccessToken = null,
                        RefreshToken = null,
                        applicationUser = null,
                        User = null,
                    };
                }
              
                var userCreated = _mapper.Map<UserDTO>(userToReturn);
                var takenAccess = _tokenService.GenerateAccessToken(userCreated);
                var refreshToken = _tokenService.GenerateRefreshToken(userCreated);
                await _tokenService.SaveToken(userCreated, refreshToken);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true, // Только для HTTP, не доступно через JavaScript
                    Secure = true,   // Использовать cookie только через HTTPS
                    MaxAge = TimeSpan.FromDays(30), // Срок действия cookie (30 дней)
                    SameSite = SameSiteMode.Strict // Защищает от CSRF атак
                };

                return new RegisterResultDTO()
                {
                    Success = true,
                    Error = null,
                    AccessToken = takenAccess,
                    RefreshToken = refreshToken,
                    applicationUser = applicationUser,
                    User = userCreated,
                };
            }
            catch (Exception ex)
            {
                return new RegisterResultDTO()
                {
                    Success = false,
                    Error = ex.Message,
                    AccessToken = null,
                    RefreshToken = null,
                    applicationUser = null,
                    User = null,
                };
            }
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO requestDTO)
        {
            try
            {
                string email = requestDTO.Email.ToUpper();

                var user = await _authRepository.GetUserByEmailAsync(email);

                if (user == null)
                {
                    return new LoginResponseDTO()
                    {
                        Success = false,
                        Error = "This User does not exist",
                        AccessToken = null,
                        RefreshToken = null,
                    };
                }

                bool isValid = await _userManager.CheckPasswordAsync(user, requestDTO.Password);

                if (isValid == false)
                {
                    return new LoginResponseDTO()
                    {
                        Success = false,
                        Error = "Your email or passwor does not valid",
                        AccessToken = null,
                        RefreshToken = null,
                        User = null,
                    };
                }

                var roles = await _userManager.GetRolesAsync(user);

                //сгенерить токены
                var userLogin = _mapper.Map<UserDTO>(user);
                var takenAccess = _tokenService.GenerateAccessToken(userLogin);
                var refreshToken = _tokenService.GenerateRefreshToken(userLogin);

                await _tokenService.SaveToken(userLogin, refreshToken);

                return new LoginResponseDTO()
                {
                    Success = true,
                    Error = null,
                    AccessToken = takenAccess,
                    RefreshToken = refreshToken,
                    User = userLogin,
                };
            }
            catch (Exception ex)
            {
                return new LoginResponseDTO()
                {
                    Success = false,
                    Error = ex.Message,
                    AccessToken = null,
                    RefreshToken = null,
                };
            }
        }

        public async Task<RefreshResponceDTO> RefreshAsync(RefreshRequestDTO refreshRequestDTO)
        {
            var refreshToken = refreshRequestDTO.refreshToken;
            var userDTO = refreshRequestDTO.userDTO;

            if (string.IsNullOrEmpty(refreshToken))
            {
                return new RefreshResponceDTO
                {
                    Success = false,
                    Error = "No refresh token found in cookies.",
                };
            }

           // var userData = _tokenService.ValidateRefreshToken(refreshToken);
            var tokenFromDb = await _authRepository.GetTokenAsync(refreshToken);

            if (/*userData == null ||*/ tokenFromDb == null)
            {
                return new RefreshResponceDTO
                {
                    Success = false,
                    Error = "Invalid token or token not found in the database",
                    AccessToken = null,
                    RefreshToken = null,
                    User = null,
                };
            }

            var user = await _authRepository.GetUserByIdAsync(userDTO.Id);

            if (user == null)
            {
                return new RefreshResponceDTO
                {
                    Success = false,
                    Error = "User not found",
                    AccessToken = null,
                    RefreshToken = null,
                    User = null,
                };
            }

            var tokens = _tokenService.GenerateTokens(new UserDTO
            {
                Id = user.Id.ToString(),
                Name = user.UserName
            });

            await _tokenService.SaveToken(userDTO, tokens.RefreshToken);

            return new RefreshResponceDTO
            {
                Success = true,
                Error = null,
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                User = _mapper.Map<UserDTO>(user),
            };
        }

        public async Task<LogoutResponceDTO> LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                return new LogoutResponceDTO
                {
                    Success = false,
                    Error = "No refresh token found in cookies.",
                };
            }

            var tokenEntity = await _authRepository.GetTokenAsync(refreshToken);

            if (tokenEntity == null)
            {
                return new LogoutResponceDTO
                {
                    Success = false,
                    Error = "Invalid refresh token."
                };
            }

            await _authRepository.RemoveTokenAsync(tokenEntity);

            return new LogoutResponceDTO
            {
                Success = true,
                Error = null,
            };
        }

        public async Task<ConfirmEmailResponceDTO> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return new ConfirmEmailResponceDTO
                {
                    Success = false,
                    Message = "Invalid user."
                };
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                return new ConfirmEmailResponceDTO
                {
                    Success = false,
                    Message = "Email confirmation error."
                };
            }

            return new ConfirmEmailResponceDTO
            {
                Success = true,
                Message = " The email has been successfully verified."
            };

        }
    }
}
