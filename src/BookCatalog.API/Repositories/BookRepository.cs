﻿using BookCatalog.API.Extensions;
using BookCatalog.API.Infrastructure;
using BookCatalog.API.Model;
using BookCatalog.API.Queries.Mappers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookCatalog.API.Repositories
{
    public class BookRepository : GenericRepository<Book>, IRepository<Book>
    {
        private const string BOOK_ID_PREFIX = "BookID_";
        protected readonly ILogger<BookRepository> logger;
        public BookRepository(BookContext context, ILogger<BookRepository> logger) : base(context)
        {
            this.logger = logger;
            this.context = context;
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
                => EF.Functions.ILike(b.Title, '%' + searchWord + '%')
                || EF.Functions.ILike(b.AuthorName, '%' + searchWord + '%')
                || EF.Functions.ILike(b.Description, '%' + searchWord + '%')
                )
                .OrderBy(b => b.Id);

            var totalItems = await query.LongCountAsync();

            if (pageIndex >= 0 && pageSize > 0)
            {
                query = (IOrderedQueryable<Book>)query.Skip(pageIndex * pageSize).Take(pageSize);
            }

            var itemsInPage = await query.ToListAsync();

            return new PaginatedItems<Book>(
                pageIndex,
                pageSize,
                totalItems,
                itemsInPage);
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

            var itemsInPage = await query.ToListAsync();

            return new PaginatedItems<Book>(
                pageIndex,
                pageSize,
                totalItems,
                itemsInPage);
        }

        public override async Task<PaginatedItems<Book>> GetAllAsync(
            int pageIndex = 0,
            int pageSize = 0)
        {
            var query = context.Set<Book>().AsQueryable()
                 .OrderBy(book => book.Id);

            var totalItems = await query.LongCountAsync();


            if (pageIndex >= 0 && pageSize > 0)
            {
                query = (IOrderedQueryable<Book>)query.Skip(pageIndex * pageSize).Take(pageSize);
            }

            var itemsInPage = await query.ToListAsync();

            return new PaginatedItems<Book>(
                pageIndex,
                pageSize,
                totalItems,
                itemsInPage);
        }

        public override async Task<Book?> GetItemByIdAsync(long id)
        {
            var query = context.Set<Book>().AsQueryable()
                .Where(book => book.Id == id)
                .Include(book => book.Publisher)
                .Include(book => book.Format)
                .Include(book => book.BookGenres)
                    .ThenInclude(bg => bg.Genre);

            return await query.FirstOrDefaultAsync();
        }

        public async override Task Update(Book updateBook)
        {
            context.ChangeTracker.Clear();

            var currentBook = await context.Set<Book>().AsQueryable()
                .Where(book => book.Id == updateBook.Id)
                .Include(book => book.BookGenres)
                .FirstOrDefaultAsync();

            if (currentBook != null)
            {
                logger.LogInformation($"Began updating book with id: {updateBook.Id}");
                BookMapper.MapBookToBook(currentBook, updateBook);

                context.TryUpdateManyToMany(currentBook.BookGenres, updateBook.BookGenres, book => book.GenreId);

                await context.SaveChangesAsync();
                logger.LogInformation($"Book with book id {updateBook.Id} successfully");
            }
            else
            {
                logger.LogInformation($"Book with {updateBook.Id} not found");
                throw new Exception("The book for update is not found");
            }
        }

        public async override Task Remove(Book removeBook)
        {
            context.ChangeTracker.Clear();

            var currentBook = await context.Set<Book>().AsQueryable()
                .Where(book => book.Id == removeBook.Id)
                .FirstOrDefaultAsync();

            if (currentBook == null)
            {
                logger.LogInformation($"Book with {removeBook.Id} not found");
                throw new Exception("The book for update is not found");
            }
            else
            {
                logger.LogInformation($"Began removing book with id: {removeBook.Id}");
                context.Remove(new Book { Id = removeBook.Id });
                logger.LogInformation($"Succesfully removed book with id: {removeBook.Id}");
            }
        }

        public async override Task<List<string>> GetConstants()
        {
            return await context.Books
                .Select(b => b.LanguageCode)
                .Distinct()
                .ToListAsync();
        }
    }
}
