using MusicWebAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Domain.Interfaces.Services
{
    public interface IUserService
    {
        Task<User> RegisterUser(User user, string password);
        Task<string> LoginUser(string email, string password);
    }
}
