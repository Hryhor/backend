namespace CommentsApp.Models.DTO
{
    public class CommentDTO
    {

        public int Id { get; set; }
        
        public string Text { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public string UserName { get; set; }
        public int? ParentId { get; set; }
        public string Email { get; set; }
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public List<CommentDTO> Replies { get; set; }
    }
}
