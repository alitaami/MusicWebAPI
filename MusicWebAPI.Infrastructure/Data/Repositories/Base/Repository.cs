using Common.Utilities;
using Microsoft.EntityFrameworkCore;
using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using MusicWebAPI.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MusicWebAPI.Infrastructure.Data.Repositories.Base
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly MusicDbContext DbContext;
        public DbSet<TEntity> Entities { get; }
        public virtual IQueryable<TEntity> Table => Entities;
        public virtual IQueryable<TEntity> TableNoTracking => Entities.AsNoTracking();

        public Repository(MusicDbContext dbContext)
        {
            DbContext = dbContext;
            Entities = DbContext.Set<TEntity>();
        }

        #region Async Method
        public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));
            Entities.Add(entity);
            if (saveNow)
                await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        public virtual async Task<TEntity> AddAndGetAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));
            Entities.Add(entity);
            if (saveNow)
                await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return entity;
        }
        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));
            await Entities.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
            if (saveNow)
                await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));
            Entities.Update(entity);
            if (saveNow)
                await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));
            Entities.UpdateRange(entities);
            if (saveNow)
                await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));
            Entities.Remove(entity);
            if (saveNow)
                await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        public virtual async Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));
            Entities.RemoveRange(entities);
            if (saveNow)
                await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        #endregion

        #region Sync Methods
        public virtual void Add(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Assert.NotNull(entity, nameof(entity));
            Entities.Add(entity);
            if (saveNow)
                DbContext.SaveChanges();
        }
        public virtual void AddRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Assert.NotNull(entities, nameof(entities));
            Entities.AddRange(entities);
            if (saveNow)
                DbContext.SaveChanges();
        }

        public virtual void Update(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Assert.NotNull(entity, nameof(entity));
            DbContext.Entry(entity).State = EntityState.Modified;
            if (saveNow)
                DbContext.SaveChanges();
        }
        public virtual void UpdateRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));
            Entities.UpdateRange(entities);
            if (saveNow)
                DbContext.SaveChanges();
        }

        public virtual TEntity GetById(CancellationToken cancellationToken, params object[] ids)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Entities.Find(ids);
        }
        public virtual async Task<TEntity> GetByIdAsync(CancellationToken cancellationToken, params object[] ids)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await Entities.FindAsync(ids, cancellationToken);
        }
        public IQueryable<TEntity> Get(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Entities;
        }

        public virtual void Delete(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Assert.NotNull(entity, nameof(entity));
            Entities.Remove(entity);
            if (saveNow)
                DbContext.SaveChanges();
        }
        public virtual void DeleteRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Assert.NotNull(entities, nameof(entities));
            Entities.RemoveRange(entities);
            if (saveNow)
                DbContext.SaveChanges();
        }
        #endregion

        #region Attach & Detach
        public virtual void Detach(TEntity entity)
        {
            Assert.NotNull(entity, nameof(entity));
            var entry = DbContext.Entry(entity);
            if (entry?.State != EntityState.Detached)
                entry.State = EntityState.Detached;
        }

        public virtual void Attach(TEntity entity)
        {
            Assert.NotNull(entity, nameof(entity));
            var entry = DbContext.Entry(entity);
            if (entry.State == EntityState.Detached)
                Entities.Attach(entity);
        }
        #endregion

        #region Explicit Loading
        public virtual async Task LoadCollectionAsync<TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> collectionProperty, CancellationToken cancellationToken)
            where TProperty : class
        {
            Attach(entity);

            var collection = DbContext.Entry(entity).Collection(collectionProperty);
            if (!collection.IsLoaded)
                await collection.LoadAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual void LoadCollection<TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> collectionProperty)
            where TProperty : class
        {
            Attach(entity);
            string propertyName = GetPropertyName(collectionProperty);
            var collection = DbContext.Entry(entity).Collection(propertyName);
            if (!collection.IsLoaded)
                collection.Load();
        }

        public virtual async Task LoadReferenceAsync<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> referenceProperty, CancellationToken cancellationToken)
            where TProperty : class
        {
            Attach(entity);
            var reference = DbContext.Entry(entity).Reference(referenceProperty);
            if (!reference.IsLoaded)
                await reference.LoadAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual void LoadReference<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> referenceProperty)
            where TProperty : class
        {
            Attach(entity);
            var reference = DbContext.Entry(entity).Reference(referenceProperty);
            if (!reference.IsLoaded)
                reference.Load();
        }
        #endregion

        private string GetPropertyName<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            if (propertyExpression.Body is MemberExpression memberExpression)
                return memberExpression.Member.Name;
            throw new ArgumentException("Invalid property expression", nameof(propertyExpression));
        }
    }
}
