using DormitoryManagementSystem.DTO.Rooms;

namespace DormitoryManagementSystem.BUS.Interfaces
{
    public interface IRoomBUS
    {
        // Basic Read
        Task<IEnumerable<RoomReadDTO>> GetAllRoomsAsync();
        Task<IEnumerable<RoomReadDTO>> GetAllRoomsIncludingInactivesAsync();
        Task<RoomReadDTO?> GetRoomByIDAsync(string id);

        // Helper Read
        Task<RoomDetailDTO?> GetRoomDetailByIDAsync(string id);
        Task<IEnumerable<int>> GetRoomCapacitiesAsync();
        IEnumerable<RoomPriceDTO> GetPriceRanges();

        // Search Methods (Consolidated in implementation)
        Task<IEnumerable<RoomReadDTO>> GetRoomsByBuildingIDAsync(string buildingId);
        Task<IEnumerable<RoomReadDTO>> SearchRoomsAsync(string keyword); // Admin Search

        // Student Search
        Task<IEnumerable<RoomDetailDTO>> SearchRoomInCardAsync(
            string? buildingName, int? roomNumber, int? capacity,
            decimal? minPrice, decimal? maxPrice, bool? allowCooking, bool? airConditioner);

        Task<IEnumerable<RoomGridDTO>> SearchRoomInGridAsync(
             string? buildingId, int? roomNumber, int? capacity, decimal? minPrice, decimal? maxPrice);

        // Transactions
        Task<string> AddRoomAsync(RoomCreateDTO dto);
        Task UpdateRoomAsync(string id, RoomUpdateDTO dto);
        Task DeleteRoomAsync(string id);
    }
}