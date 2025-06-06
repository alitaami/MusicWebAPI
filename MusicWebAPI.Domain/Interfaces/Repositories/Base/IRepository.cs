using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MusicWebAPI.Domain.Interfaces.Repositories.Base
{
    public interface IRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> Table { get; }
        IQueryable<TEntity> TableNoTracking { get; }

        #region Async
        Task AddAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true);
        Task<TEntity> AddAndGetAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true);
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true);

        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true);

        Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true);

        Task<TEntity> GetByIdAsync(CancellationToken cancellationToken, params object[] ids);

        Task DeleteAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true);
        Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true);
        #endregion

        #region Sync
        void Add(TEntity entity, CancellationToken cancellationToken, bool saveNow = true);
        void AddRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true);

        void Update(TEntity entity, CancellationToken cancellationToken, bool saveNow = true);
        void UpdateRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true);

        IQueryable<TEntity> Get(CancellationToken cancellationToken);
        TEntity GetById(CancellationToken cancellationToken, params object[] ids);

        void Delete(TEntity entity, CancellationToken cancellationToken, bool saveNow = true);
        void DeleteRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true);
        #endregion

        #region Attach
        void Attach(TEntity entity);
        void Detach(TEntity entity);
        #endregion

        #region ExplicitLoading
        Task LoadCollectionAsync<TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> collectionProperty, CancellationToken cancellationToken) where TProperty : class;
        void LoadCollection<TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> collectionProperty) where TProperty : class;
        Task LoadReferenceAsync<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> referenceProperty, CancellationToken cancellationToken) where TProperty : class;
        void LoadReference<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> referenceProperty) where TProperty : class;
        #endregion
    }
}
