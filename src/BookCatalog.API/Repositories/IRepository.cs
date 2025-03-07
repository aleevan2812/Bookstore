﻿using BookCatalog.API.Model;
using System.Linq.Expressions;

namespace BookCatalog.API.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task AddAsync(T entity);
        Task<T> GetItemByIdAsync(long id);
        Task<PaginatedItems<T>> SearchAsync(string searchWord, int pageIndex = 0, int pageSize = 10);
        Task<PaginatedItems<T>> GetAllAsync(int pageIndex = 0, int pageSize = 10);
        Task<PaginatedItems<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            int pageIndex = 0,
            int pageCount = 0);
        Task Update(T entity);
        Task Remove(T entity);
        Task SaveChangesAsync();
        Task<long> LongCountAsync();
        Task<List<string>> GetConstants();
    }
}
