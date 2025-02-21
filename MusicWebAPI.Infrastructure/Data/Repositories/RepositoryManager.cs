using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using MusicWebAPI.Domain.Interfaces.Repositories;
using MusicWebAPI.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicWebAPI.Infrastructure.Data.Repositories
{
    public class RepositoryManager : IRepositoryManager
    {
        private MusicDbContext _context;
        // private IUserRepository _userRepository;

        public RepositoryManager(MusicDbContext context)
        {
            _context = context;
        }
        #region Cross-Repository query
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

        public async Task<DbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return (DbContextTransaction)await _context.Database.BeginTransactionAsync(cancellationToken);
        }
        public DbContextTransaction BeginTransaction()
        {
            return (DbContextTransaction)_context.Database.BeginTransaction();
        }

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
        //public IUserRepository User
        //{
        //    get
        //    {
        //        if (this._userRepository == null)
        //            this._userRepository = new UserRepository(_context);

        //        return this._userRepository;
        //    }
        //}

        #endregion
    }

}
