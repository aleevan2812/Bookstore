﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BookCatalog.API.Extensions;
using BookCatalog.API.Model;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.API.Infrastructure.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        protected PostgresContext context;

        public GenericRepository(PostgresContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public virtual async Task AddAsync(T entity)
        {
            await context.AddAsync(entity);
        }

        public virtual async Task<PaginatedItems<T>> SearchAsync(
            string searchWord,
            int pageIndex = 0,
            int pageSize = 10
            )
        {
            throw new NotImplementedException();
        }

        public async virtual Task<PaginatedItems<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            int pageIndex = 0,
            int pageSize = 0)
        {
            var query = context.Set<T>().AsQueryable().Where(predicate);

            var totalItems = await query.LongCountAsync();

            if (pageIndex >= 0 && pageSize > 0)
            {
                query = query.Skip(pageIndex * pageSize).Take(pageSize);
            }

            return new PaginatedItems<T>(pageIndex, pageSize, totalItems, await query.ToListAsync());
        }

        public virtual async Task<T> GetItemByIdAsync(long id)
        {
            return await context.FindAsync<T>(id);
        }

        public virtual async Task<PaginatedItems<T>> GetAllAsync(int pageIndex = 0, int pageSize = 0)
        {
            var query = context.Set<T>().AsQueryable();

            var totalItems = await query.LongCountAsync();

            if (pageIndex >= 0 && pageSize > 0)
            {
                query = query.Skip(pageIndex * pageSize).Take(pageSize);
            }

            return new PaginatedItems<T>(pageIndex, pageSize, totalItems, await query.ToListAsync());
        }

        public virtual async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }

        public virtual void Update(T entity)
        {
            context.Update(entity);
        }

        public virtual void Remove(T entity)
        {
            context.Remove(entity);
        }

        public async Task<long> LongCountAsync()
        {
            return await context.Set<T>().LongCountAsync();
        }
    }
}
