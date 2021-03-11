using Core.Entities;
using Core.Interfaces;
using Infrastructure.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        public UnitOfWork(StoreContext context)
        {
            Context = context;
        }

        private StoreContext Context;
        private Hashtable Repositories;
        public async Task<int> Complete()
        {
            return await Context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Context.Dispose();
        }

        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
        {
            if (Repositories == null) Repositories = new Hashtable();

            var type = typeof(TEntity).Name;
            if(!Repositories.ContainsKey(type))
            {
                var repositoryType = typeof(GenericRepository<>);
                var repositoryInstance = Activator.CreateInstance(repositoryType.
                    MakeGenericType(typeof(TEntity)), Context);
                Repositories.Add(type, repositoryInstance);
            }
            return (IGenericRepository<TEntity>)Repositories[type];
        }
    }
}
