namespace CommentsApp.Models.DTO
{
    public class UpdateCommentDTO
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
