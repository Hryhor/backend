using System.ComponentModel.DataAnnotations;

namespace CommentsApp.Models.DTO
{
    public class CommentCreateDTO
    {
        public int? ParentId { get; set; }
        public string Text { get; set; }
       // public IFormFile? file { get; set; }
    }
}
