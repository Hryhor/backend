using CommentsApp.Data;
using CommentsApp.Models;
using CommentsApp.Repository.IRepository;

namespace CommentsApp.Repository
{
    public class CommentRepository : Repository<Comment>, ICommentRepository
    {
        private readonly ApplicationDbContext _db;

        public CommentRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Comment> UpdateAsync(Comment entity)
        {
            entity.UpdatedDate = DateTime.Now;
            _db.Comments.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
    }
}
