using System;
using System.Threading.Tasks;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.IdentityServer.WebApi.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AVStack.IdentityServer.WebApi.Data.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : EntityBase
    {
        private readonly AccountDbContext _dbContext;
        private readonly DbSet<TEntity> _entities;

        public GenericRepository(AccountDbContext dbContext)
        {
            _dbContext = dbContext;
            _entities = dbContext.Set<TEntity>();
        }

        public async Task<TEntity> FindById(Guid id) => await _entities.SingleOrDefaultAsync(e => e.Id == id);

        public async Task Insert(TEntity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            _entities.Add(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(TEntity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            _entities.Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(TEntity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            _entities.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}