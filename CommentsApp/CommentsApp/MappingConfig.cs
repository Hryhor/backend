using CommentsApp.Models.DTO;
using CommentsApp.Models;
using AutoMapper;

namespace CommentsApp
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<ApplicationUser, UserDTO>().ReverseMap();
            CreateMap<Comment, CommentCreateDTO>().ReverseMap();
            CreateMap<Comment, UpdateCommentDTO>().ReverseMap();
        }
    }
}
