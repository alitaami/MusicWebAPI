using MusicWebAPI.Domain.Interfaces.Repositories.Base;
using MusicWebAPI.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MusicWebAPI.Infrastructure.Data.Repositories.Base
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly MusicDbContext _context;

        public Repository(MusicDbContext context)
        {
            _context = context;
        }

        #region Table Properties
        public IQueryable<TEntity> Table => _context.Set<TEntity>();
        public IQueryable<TEntity> TableNoTracking => _context.Set<TEntity>().AsNoTracking();

        #endregion

        #region Async Methods

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            await _context.Set<TEntity>().AddAsync(entity, cancellationToken);
            if (saveNow)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            await _context.Set<TEntity>().AddRangeAsync(entities, cancellationToken);
            if (saveNow)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<TEntity> GetByIdAsync(CancellationToken cancellationToken, params object[] ids)
        {
            return await _context.Set<TEntity>().FindAsync(ids, cancellationToken);
        }

        public async Task<IEnumerable<TEntity>> GetListAsync(CancellationToken cancellationToken, Expression<Func<TEntity, bool>> predicate = null)
        {
            if (predicate == null)
            {
                return await _context.Set<TEntity>().ToListAsync(cancellationToken);
            }

            return await _context.Set<TEntity>().Where(predicate).ToListAsync(cancellationToken);
        }

        public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            _context.Set<TEntity>().Remove(entity);
            if (saveNow)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            _context.Set<TEntity>().RemoveRange(entities);
            if (saveNow)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            _context.Set<TEntity>().Update(entity);
            if (saveNow)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            _context.Set<TEntity>().UpdateRange(entities);
            if (saveNow)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        #endregion

        #region Sync Methods

        public void Add(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            _context.Set<TEntity>().Add(entity);
            if (saveNow)
            {
                _context.SaveChanges();
            }
        }

        public void AddRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            _context.Set<TEntity>().AddRange(entities);
            if (saveNow)
            {
                _context.SaveChanges();
            }
        }

        public IQueryable<TEntity> Get(CancellationToken cancellationToken, Expression<Func<TEntity, bool>> predicate = null)
        {
            if (predicate == null)
            {
                return _context.Set<TEntity>();
            }

            return _context.Set<TEntity>().Where(predicate);
        }

        public TEntity GetById(CancellationToken cancellationToken, params object[] ids)
        {
            return _context.Set<TEntity>().Find(ids);
        }

        public void Delete(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            _context.Set<TEntity>().Remove(entity);
            if (saveNow)
            {
                _context.SaveChanges();
            }
        }

        public void DeleteRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            _context.Set<TEntity>().RemoveRange(entities);
            if (saveNow)
            {
                _context.SaveChanges();
            }
        }

        public void Update(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            _context.Set<TEntity>().Update(entity);
            if (saveNow)
            {
                _context.SaveChanges();
            }
        }

        public void UpdateRange(IEnumerable<TEntity> entities, CancellationToken cancellationToken, bool saveNow = true)
        {
            _context.Set<TEntity>().UpdateRange(entities);
            if (saveNow)
            {
                _context.SaveChanges();
            }
        }

        #endregion

        #region Attach / Detach

        public void Attach(TEntity entity)
        {
            _context.Set<TEntity>().Attach(entity);
        }

        public void Detach(TEntity entity)
        {
            _context.Entry(entity).State = (Microsoft.EntityFrameworkCore.EntityState)EntityState.Detached;
        }

        #endregion

        #region Explicit Loading

        public void LoadCollection<TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> collectionProperty) where TProperty : class
        {
            _context.Entry(entity).Collection(collectionProperty).Load();
        }

        public void LoadReference<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> referenceProperty) where TProperty : class
        {
            _context.Entry(entity).Reference(referenceProperty).Load();
        }

        #endregion

    }
}