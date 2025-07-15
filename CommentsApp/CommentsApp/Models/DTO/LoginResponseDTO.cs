using Microsoft.AspNetCore.Authentication.BearerToken;

namespace CommentsApp.Models.DTO
{
    public class LoginResponseDTO
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public UserDTO? User { get; set; }
    }
}
