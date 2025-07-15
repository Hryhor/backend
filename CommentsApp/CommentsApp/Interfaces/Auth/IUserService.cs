using CommentsApp.Models.DTO;

namespace CommentsApp.Interfaces.Auth
{
    public interface IUserService
    {
        Task<RegisterResultDTO> RegisterAsync(RegisterRequestDTO requestDTO);
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO requestDTO);
        Task<LogoutResponceDTO> LogoutAsync(string refreshToken);
        Task<RefreshResponceDTO> RefreshAsync(RefreshRequestDTO refreshRequestDTO);
        Task<ConfirmEmailResponceDTO> ConfirmEmailAsync(string userId, string token);
    }
}
