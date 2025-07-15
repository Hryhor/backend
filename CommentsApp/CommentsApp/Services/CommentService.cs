using AutoMapper;
using CommentsApp.Interfaces;
using CommentsApp.Migrations;
using CommentsApp.Models;
using CommentsApp.Models.DTO;
using CommentsApp.Repository;
using CommentsApp.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CommentsApp.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IMapper _mapper;

        public CommentService(ICommentRepository commentRepository, IMapper mapper)
        {
            _mapper = mapper;
            _commentRepository = commentRepository;
        }

        public async Task<List<CommentDTO>> GetComments(int pageSize = 25, int pageNumber = 1,
            string? sortBy = null, bool descending = false)
        {
            var allComments = await _commentRepository.GetAllAsync(
                includeProperties: "ApplicationUser",
                pageSize: int.MaxValue,
                pageNumber: 1
            );

            var commentDict = allComments.ToDictionary(c => c.Id);
            var rootComments = new List<Comment>();

            foreach (var comment in allComments)
            {
                comment.Replies = new List<Comment>();
            }

            foreach (var comment in allComments)
            {
                if (comment.ParentId is null)
                {
                    rootComments.Add(comment);
                }
                else if (commentDict.TryGetValue(comment.ParentId.Value, out var parent))
                {
                    if (parent.Replies == null)
                        parent.Replies = new List<Comment>();

                    parent.Replies.Add(comment);
                }
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "username":
                        rootComments = descending
                            ? rootComments.OrderByDescending(c => c.ApplicationUser.UserName).ToList()
                            : rootComments.OrderBy(c => c.ApplicationUser.UserName).ToList();
                        break;
                    case "email":
                        rootComments = descending
                            ? rootComments.OrderByDescending(c => c.ApplicationUser.Email).ToList()
                            : rootComments.OrderBy(c => c.ApplicationUser.Email).ToList();
                        break;
                    case "createddate":
                        rootComments = descending
                            ? rootComments.OrderByDescending(c => c.CreatedDate).ToList()
                            : rootComments.OrderBy(c => c.CreatedDate).ToList();
                        break;
                }
            }

            var pagedRoot = rootComments.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            List<CommentDTO> MapToDto(List<Comment> comments)
            {
                return comments.Select(t => new CommentDTO
                {
                    Id = t.Id,
                    UserName = t.ApplicationUser?.UserName,
                    Text = t.Text,
                    CreatedDate = t.CreatedDate,
                    ParentId = t.ParentId,
                    Email = t.ApplicationUser?.Email,
                    FilePath = t.FilePath,
                    FileName = t.FileName,
                    ContentType = t.ContentType,
                    Replies = MapToDto(t.Replies?.OrderBy(c => c.CreatedDate).ToList() ?? new List<Comment>())
                }).ToList();
            }

            return MapToDto(pagedRoot);
        }


        public async Task<CommentCreateDTO> CreateComment(CommentCreateDTO commentCreateDTO, string userId, IFormFile? file)
        {
            //var file = commentCreateDTO.file;
            Comment comment = _mapper.Map<Comment>(commentCreateDTO);
            comment.ApplicationUserId = userId;
            comment.CreatedDate = DateTime.UtcNow;

            if (file != null)
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);


                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                comment.FileName = file.FileName;
                comment.FilePath = $"/Uploads/{fileName}";
                comment.ContentType = file.ContentType;
            }

            await _commentRepository.CreateAsync(comment);

            var createdComment = await _commentRepository.GetAsync(x => x.Id == comment.Id, includeProperties: "ApplicationUser");

            CommentCreateDTO resultDto = _mapper.Map<CommentCreateDTO>(createdComment);
            return resultDto;
        }

        public async Task<bool> DeleteComment(int id)
        {
            var comment = await _commentRepository.GetAsync(x => x.Id == id);

            if (comment == null)
                return false;

            await _commentRepository.RemoveAsync(comment);

            return true;
        }

        public async Task<bool> UpdateComment(UpdateCommentDTO updateCommentDTO)
        {
            Comment comment = _mapper.Map<Comment>(updateCommentDTO);

            var updatedComment = await _commentRepository.UpdateAsync(comment);

            return updatedComment != null;
        }
    }
}
