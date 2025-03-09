using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MusicWebAPI.Application.ViewModels.UserViewModel;

namespace MusicWebAPI.Application.Commands
{
    public class RegisterUserCommand : IRequest<RegisterUserViewModel>
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must be at least 8 characters long, contain one uppercase letter, one lowercase letter, one digit, and one special character.")]
        public string Password { get; set; }

        [Required]
        public string FullName { get; set; }

        public bool IsArtist { get; set; } = false;
    }
}
