using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CommentsApp.Models
{
    public class Comment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }


        [Required]
        public string ApplicationUserId { get; set; }

        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser ApplicationUser { get; set; }

        public int? ParentId { get; set; } 
        public Comment Parent { get; set; }

        public List<Comment> Replies { get; set; } = new();

        public string? FileName { get; set; } 
        public string? FilePath { get; set; } 
        public string? ContentType { get; set; } 
    }
}
