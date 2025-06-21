using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Interfaces.Repositories;
using MusicWebAPI.Infrastructure.Data.Context;
using MusicWebAPI.Infrastructure.Data.Repositories.Base;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicWebAPI.Core.Resources;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using static MusicWebAPI.Domain.Base.Exceptions.CustomExceptions;

namespace MusicWebAPI.Infrastructure.Data.Repositories
{
    public class UserFavoriteRepository : Repository<UserFavorite>, IUserFavoriteRepository
    {
        private MusicDbContext _context;
        public UserFavoriteRepository(MusicDbContext context) : base(context)
        {
            _context = context;
        }


    }
}
