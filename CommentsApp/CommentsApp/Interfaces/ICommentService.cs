using CommentsApp.Models.DTO;


namespace CommentsApp.Interfaces
{
    public interface ICommentService
    {
        public Task<List<CommentDTO>> GetComments(int pageSize = 25, int pageNumber = 1, string? sortBy = null, bool descending = false);

        public Task<CommentCreateDTO> CreateComment(CommentCreateDTO commentCreateDTO, string userId, IFormFile? file);
        public Task<bool> DeleteComment(int id);

        public Task<bool> UpdateComment(UpdateCommentDTO updateCommentDTO);
  
    }
}
