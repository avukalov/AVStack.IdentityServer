using System;
using System.Threading.Tasks;
using AVStack.IdentityServer.WebApi.Data.Entities;

namespace AVStack.IdentityServer.WebApi.Data.Repositories.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : EntityBase
    {
        Task<TEntity> FindById(Guid id);
        Task Insert(TEntity entity);
        Task Update(TEntity entity);
        Task Delete(TEntity entity);
    }
}