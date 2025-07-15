using CommentsApp.Models;

namespace CommentsApp.Repository.IRepository
{
    public interface ICommentRepository : IRepository<Comment>
    {
        Task<Comment> UpdateAsync(Comment entity);
    }
}
