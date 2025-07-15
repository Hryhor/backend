using CommentsApp.Interfaces.Auth;

namespace CommentsApp.Services
{
    public class PasswordService : IPasswordService
    {
        public string Generate(string password) => BCrypt.Net.BCrypt.EnhancedHashPassword(password);

        public bool Verify(string password, string hashedPassword) => BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword);
    }
}
