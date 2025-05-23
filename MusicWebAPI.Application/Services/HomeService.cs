using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MusicWebAPI.Core;
using MusicWebAPI.Domain.Entities;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using MusicWebAPI.Domain.Interfaces.Services;
using MusicWebAPI.Infrastructure.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Application.Services
{
    public class HomeService : IHomeService
    {
        private readonly IMapper _mapper;
        private IRepositoryManager _repositoryManager;

        public HomeService(IRepositoryManager repositoryManager, IMapper mapper)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<object>> GetSongs(string term, int pageSize, int pageNumber, CancellationToken cancellationToken)
        {
            return await _repositoryManager.Song.GetSongsByTerm(term, pageSize, pageNumber, cancellationToken);
        }
        public async Task<PaginatedResult<object>> GetPopularSongs(int pageSize, int pageNumber, CancellationToken cancellationToken)
        {
            return await _repositoryManager.Song.GetPopularSongs(pageSize, pageNumber, cancellationToken);
        }
    }
}
