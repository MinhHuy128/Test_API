using DormitoryManagementSystem.DAO.Context;
using DormitoryManagementSystem.DAO.Interfaces;
using DormitoryManagementSystem.DTO.SearchCriteria; // Criteria
using DormitoryManagementSystem.Utils; // AppConstants
using DormitoryManagementSystem.Entity;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagementSystem.DAO.Implementations
{
    public class RoomDAO : IRoomDAO
    {
        private readonly PostgreDbContext _context;
        public RoomDAO(PostgreDbContext context) => _context = context;

        // --- CRUD ---
        public async Task<IEnumerable<Room>> GetAllRoomsIncludingInactivesAsync() =>
            await _context.Rooms.AsNoTracking().ToListAsync();

        public async Task<Room?> GetRoomByIDAsync(string id) => await _context.Rooms.FindAsync(id);

        public async Task AddRoomAsync(Room room)
        {
            await _context.Rooms.AddAsync(room);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRoomAsync(Room room)
        {
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRoomAsync(string id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                room.Status = AppConstants.RoomStatus.Inactive; // Soft Delete
                await _context.SaveChangesAsync();
            }
        }

        // --- HELPER ---
        public async Task<Room?> GetRoomDetailByIDAsync(string id) =>
            await _context.Rooms.AsNoTracking()
                                .Include(r => r.Building)
                                .FirstOrDefaultAsync(r => r.Roomid == id);

        public async Task<IEnumerable<int>> GetDistinctCapacitiesAsync() =>
            await _context.Rooms.AsNoTracking().Select(r => r.Capacity).Distinct().OrderBy(c => c).ToListAsync();

        // --- MAIN SEARCH FUNCTION ---
        public async Task<IEnumerable<Room>> SearchRoomsAsync(RoomSearchCriteria criteria)
        {
            var query = _context.Rooms.AsNoTracking()
                .Include(r => r.Building)
                .AsQueryable();

            // 1. Filter by Status (Default: exclude Inactive if criteria is null/empty)
            if (!string.IsNullOrEmpty(criteria.Status))
                query = query.Where(r => r.Status == criteria.Status);
            else
                query = query.Where(r => r.Status != AppConstants.RoomStatus.Inactive);

            // 2. Filters
            if (!string.IsNullOrEmpty(criteria.BuildingID) && criteria.BuildingID != "All")
                query = query.Where(r => r.Buildingid == criteria.BuildingID);

            if (criteria.RoomNumber.HasValue)
                query = query.Where(r => r.Roomnumber == criteria.RoomNumber.Value);

            if (criteria.Capacity.HasValue && criteria.Capacity > 0)
                query = query.Where(r => r.Capacity == criteria.Capacity.Value);

            if (criteria.MinPrice.HasValue)
                query = query.Where(r => r.Price >= criteria.MinPrice.Value);

            if (criteria.MaxPrice.HasValue)
                query = query.Where(r => r.Price <= criteria.MaxPrice.Value);

            if (criteria.AllowCooking.HasValue)
                query = query.Where(r => r.Allowcooking == criteria.AllowCooking.Value);

            if (criteria.AirConditioner.HasValue)
                query = query.Where(r => r.Airconditioner == criteria.AirConditioner.Value);

            // 3. Keyword Search
            if (!string.IsNullOrWhiteSpace(criteria.Keyword))
            {
                string key = criteria.Keyword.ToLower().Trim();
                query = query.Where(r => r.Roomid.ToLower().Contains(key) ||
                                         r.Roomnumber.ToString().Contains(key));
            }

            return await query.OrderBy(r => r.Buildingid).ThenBy(r => r.Roomnumber).ToListAsync();
        }
    }
}