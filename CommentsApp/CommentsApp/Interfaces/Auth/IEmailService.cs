namespace CommentsApp.Interfaces.Auth
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
