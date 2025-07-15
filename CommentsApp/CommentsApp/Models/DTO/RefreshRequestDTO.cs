namespace CommentsApp.Models.DTO
{
    public class RefreshRequestDTO
    {
        public string refreshToken { get; set; }
        public UserDTO userDTO { get; set; }
    }
}
