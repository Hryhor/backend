using CommentsApp.Models;
using CommentsApp.Models.DTO;
using System.Security.Claims;

namespace CommentsApp.Interfaces.Auth
{
    public interface ITokenService
    {
        Tokens GenerateTokens(UserDTO user);
        string GenerateAccessToken(UserDTO user);
        string GenerateRefreshToken(UserDTO user);
        Task SaveToken(UserDTO user, string refreshToken);
        Task DeleteToken(string token);
        string ValidateAccessToken(string token);
        string ValidateRefreshToken(string token);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
