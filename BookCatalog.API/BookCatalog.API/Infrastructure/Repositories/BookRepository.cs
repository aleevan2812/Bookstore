﻿using BookCatalog.API.DTOs;
using BookCatalog.API.Extensions;
using BookCatalog.API.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

namespace BookCatalog.API.Infrastructure.Repositories
{
    public class BookRepository : GenericRepository<Book>
    {
        public BookRepository(PostgresContext context) : base(context)
        {
        }

        public override async Task<PaginatedItems<Book>> SearchAsync(
            string searchWord,
            int pageIndex = 0,
            int pageSize = 10
            )
        {
            var query = context.Set<Book>().AsQueryable()
                .Where(
                b
                => b.Title.SimilarSearch(searchWord)
                || b.Description.SimilarSearch(searchWord)
                || b.AuthorName.SimilarSearch(searchWord))
                .OrderByDescending(g => EF.Functions.PgroongaScore());

            var totalItems = await query.LongCountAsync();

            if (pageIndex >= 0 && pageSize > 0)
            {
                query = (IOrderedQueryable<Book>)query.Skip(pageIndex * pageSize).Take(pageSize);
            }

            return new PaginatedItems<Book>(
                pageIndex, 
                pageSize, 
                totalItems, 
                await query.ToListAsync());
        }

        public async override Task<PaginatedItems<Book>> FindAsync(
            Expression<Func<Book, bool>> predicate,
            int pageIndex = 0,
            int pageSize = 10)
        {       

            var query = context.Set<Book>().AsQueryable()
                .Where(predicate)
                .OrderBy(book => book.Id)
                ;

            var totalItems = await query.LongCountAsync();

            if (pageIndex >= 0 && pageSize > 0)
            {
                query = (IOrderedQueryable<Book>)query.Skip(pageIndex * pageSize).Take(pageSize);
            }

            return new PaginatedItems<Book>(
                pageIndex, 
                pageSize, 
                totalItems, 
                await query.ToListAsync());
        }

        public override async Task<PaginatedItems<Book>> GetAllAsync(
            int pageIndex = 0, 
            int pageSize = 0)
        {
            var query = context.Set<Book>().AsQueryable()
                 .OrderBy(book => book.Id)
                 ;

            var totalItems = await query.LongCountAsync();


            if (pageIndex >= 0 && pageSize > 0)
            {
                query = (IOrderedQueryable<Book>)query.Skip(pageIndex * pageSize).Take(pageSize);
            }

            return new PaginatedItems<Book>(pageIndex, pageSize, totalItems, await query.ToListAsync());
        }
    }
}
