using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Domain.Interfaces.Repositories.Base
{
    public interface IRepository<TEntity> where TEntity : class
    {
        // Table properties for querying
        IQueryable<TEntity> Table { get; }
        IQueryable<TEntity> TableNoTracking { get; }

        #region Async Methods

        Task AddAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true);
        Task<TEntity> AddAndGetAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true);
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true);

        Task<TEntity> GetByIdAsync(CancellationToken cancellationToken, params object[] ids);
        Task<IEnumerable<TEntity>> GetListAsync(CancellationToken cancellationToken, Expression<Func<TEntity, bool>> predicate = null);

        Task DeleteAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true);
        Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true);

        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true);
        Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true);

        #endregion

        #region Sync Methods

        void Add(TEntity entity, CancellationToken cancellationToken, bool saveNow = true);
        void AddRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true);

        IQueryable<TEntity> Get(CancellationToken cancellationToken, Expression<Func<TEntity, bool>> predicate = null);
        TEntity GetById(CancellationToken cancellationToken, params object[] ids);
        void Delete(TEntity entity, CancellationToken cancellationToken, bool saveNow = true);
        void DeleteRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true);

        void Update(TEntity entity, CancellationToken cancellationToken, bool saveNow = true);
        void UpdateRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true);

        #endregion

        #region Attach / Detach

        void Attach(TEntity entity);
        void Detach(TEntity entity);

        #endregion

        #region Explicit Loading

        void LoadCollection<TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> collectionProperty) where TProperty : class;
        void LoadReference<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> referenceProperty) where TProperty : class;

        #endregion
    }
}
