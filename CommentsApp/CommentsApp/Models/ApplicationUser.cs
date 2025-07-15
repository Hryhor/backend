using Microsoft.AspNetCore.Identity;

namespace CommentsApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }
}
