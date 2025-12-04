using DormitoryManagementSystem.Entity;
using DormitoryManagementSystem.DTO.SearchCriteria; // RoomSearchCriteria

namespace DormitoryManagementSystem.DAO.Interfaces
{
    public interface IRoomDAO
    {
        // CRUD
        Task<IEnumerable<Room>> GetAllRoomsIncludingInactivesAsync();
        Task<Room?> GetRoomByIDAsync(string id);
        Task AddRoomAsync(Room room);
        Task UpdateRoomAsync(Room room);
        Task DeleteRoomAsync(string id);

        // Helper
        Task<Room?> GetRoomDetailByIDAsync(string id);
        Task<IEnumerable<int>> GetDistinctCapacitiesAsync();

        // SEARCH CONSOLIDATION
        Task<IEnumerable<Room>> SearchRoomsAsync(RoomSearchCriteria criteria);
    }
}