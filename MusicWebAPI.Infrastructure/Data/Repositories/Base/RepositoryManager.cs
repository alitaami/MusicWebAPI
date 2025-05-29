using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using MusicWebAPI.Domain.External.Caching;
using MusicWebAPI.Domain.Interfaces.Repositories;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using MusicWebAPI.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicWebAPI.Infrastructure.Data.Repositories.Base
{
    public class RepositoryManager : IRepositoryManager
    {
        private MusicDbContext _context;
        private ICacheService _cacheService;

        private ISongRepository _songRepository;
        private IPlayListRepository _playListRepository;
        private IPlayListSongsRepository _playListSongsRepository;

        public RepositoryManager(MusicDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        #region Cross-Repository query
        public async Task<DbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return (DbContextTransaction)await _context.Database.BeginTransactionAsync(cancellationToken);
        }
        public DbContextTransaction BeginTransaction()
        {
            return (DbContextTransaction)_context.Database.BeginTransaction();
        }
        public IEnumerable<T> GetListWithRawSql<T>(string procedureOrQuery, CancellationToken cancellationToken, params object[] parameters)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return _context.Database.SqlQueryRaw<T>(procedureOrQuery, parameters).ToList();
        }

        public T GetSingleWithRawSql<T>(string procedureOrQuery, CancellationToken cancellationToken, params object[] parameters)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return _context.Database.SqlQueryRaw<T>(procedureOrQuery, parameters).FirstOrDefault();
        }
        #endregion

        #region Save Changes
        public void SaveChanges()
        {
            _context.SaveChanges();
        }
        public void SaveChanges(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _context.SaveChanges();
        }
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        #endregion

        #region Model Repositories
        public ISongRepository Song
        {
            get
            {
                if (this._songRepository == null)
                    this._songRepository = new SongRepository(_context, _cacheService);

                return this._songRepository;
            }
        }

        public IPlayListRepository PlayList
        {
            get
            {
                if (this._playListRepository == null)
                    this._playListRepository = new PlayListRepository(_context);

                return this._playListRepository;
            }
        }

        public IPlayListSongsRepository PlayListSongs
        {
            get
            {
                if (this._playListSongsRepository == null)
                    this._playListSongsRepository = new PlayListSongsRepository(_context);

                return this._playListSongsRepository;
            }
        }

        #endregion
    }

}
