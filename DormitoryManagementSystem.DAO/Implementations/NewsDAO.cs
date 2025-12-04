using DormitoryManagementSystem.DAO.Context;
using DormitoryManagementSystem.DAO.Interfaces;
using DormitoryManagementSystem.Entity;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagementSystem.DAO.Implementations
{
    public class NewsDAO : INewsDAO
    {
        private readonly PostgreDbContext _context;
        public NewsDAO(PostgreDbContext context) => _context = context;

        public async Task<IEnumerable<News>> GetAllNewsAsync() =>
            await _context.News.AsNoTracking().Where(n => n.Isvisible == true).ToListAsync();

        public async Task<IEnumerable<News>> GetAllNewsIncludingInactivesAsync() =>
            await _context.News.AsNoTracking().ToListAsync();

        public async Task<News?> GetNewsByIDAsync(string id) => await _context.News.FindAsync(id);

        public async Task AddNewsAsync(News news)
        {
            await _context.News.AddAsync(news);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateNewsAsync(News news)
        {
            _context.News.Update(news);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteNewsAsync(string id)
        {
            var n = await _context.News.FindAsync(id);
            if (n != null)
            {
                n.Isvisible = false; // Soft delete
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<News>> GetNewsSummariesAsync() =>
            await _context.News.AsNoTracking()
                               .Where(n => n.Isvisible == true)
                               .OrderByDescending(n => n.Publisheddate)
                               .ToListAsync();
    }
}