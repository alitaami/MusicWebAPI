using AutoMapper;
using MusicWebAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Application.ViewModels
{
    public class UserViewModel
    {
        public class RegisterUserViewModel : Base.BaseViewModel<RegisterUserViewModel, User>
        {
            public string Id { get; set; }
            public string FullName { get; set; }
            public string Bio { get; set; } 
            public bool IsArtist { get; set; }
            public override void CustomMappings(IMappingExpression<User, RegisterUserViewModel> mapping)
            {
                mapping.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)).ReverseMap();
                mapping.ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName)).ReverseMap();
                mapping.ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.Bio)).ReverseMap();
                mapping.ForMember(dest => dest.IsArtist, opt => opt.MapFrom(src => src.IsArtist)).ReverseMap();
            }
        }
        public class LoginUserViewModel  
        {
            public string Token { get; set; }
        }
    }
}
