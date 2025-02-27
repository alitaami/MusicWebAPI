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
            public string FullName { get; set; }
            public string Bio { get; set; }
            public string ImageUrl { get; set; }
            public bool IsArtist { get; set; }
        }
    }
}
