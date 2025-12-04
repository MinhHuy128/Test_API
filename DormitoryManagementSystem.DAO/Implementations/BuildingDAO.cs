using DormitoryManagementSystem.DAO.Context;
using DormitoryManagementSystem.DAO.Interfaces;
using DormitoryManagementSystem.Entity;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagementSystem.DAO.Implementations
{
    public class BuildingDAO : IBuildingDAO
    {
        private readonly PostgreDbContext _context;
        public BuildingDAO(PostgreDbContext context) => _context = context;

        public async Task<IEnumerable<Building>> GetAllBuildingAsync() =>
            await _context.Buildings.AsNoTracking().Where(b => b.IsActive).ToListAsync();

        public async Task<IEnumerable<Building>> GetAllBuildingIncludingInactivesAsync() =>
            await _context.Buildings.AsNoTracking().ToListAsync();

        public async Task<Building?> GetByIDAsync(string id) => await _context.Buildings.FindAsync(id);

        public async Task AddBuildingAsync(Building building)
        {
            await _context.Buildings.AddAsync(building);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBuildingAsync(Building building)
        {
            _context.Buildings.Update(building);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBuildingAsync(string id)
        {
            var b = await _context.Buildings.FindAsync(id);
            if (b != null)
            {
                b.IsActive = false; // Soft Delete
                await _context.SaveChangesAsync();
            }
        }
    }
}