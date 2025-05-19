using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace MusicWebAPI.Domain.Interfaces.Repositories.Base
{
    public interface IRepositoryManager
    {

        #region Transaction Handling

        Task<DbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
        DbContextTransaction BeginTransaction();
        #endregion

        #region Save Changes

        void SaveChanges();
        void SaveChanges(CancellationToken cancellationToken);
        Task SaveChangesAsync(CancellationToken cancellationToken);

        #endregion

        #region Cross-Repository Queries

        IEnumerable<T> GetListWithRawSql<T>(string procedureOrQuery, CancellationToken cancellationToken, params object[] parameters);
        T GetSingleWithRawSql<T>(string procedureOrQuery, CancellationToken cancellationToken, params object[] parameters);

        #endregion

        #region Repositories
        ISongRepository Song { get; }
        IPlayListRepository PlayList { get; }
        IPlayListSongsRepository PlayListSongs { get; }

        #endregion
    }
}
