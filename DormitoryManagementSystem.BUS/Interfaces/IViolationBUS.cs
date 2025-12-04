using DormitoryManagementSystem.DTO.Violations;

namespace DormitoryManagementSystem.BUS.Interfaces
{
    public interface IViolationBUS
    {
        // Basic Read
        Task<IEnumerable<ViolationReadDTO>> GetAllViolationsAsync();
        Task<ViolationReadDTO?> GetViolationByIDAsync(string id);

        // Search Wrappers (Gọi SearchDAO)
        Task<IEnumerable<ViolationReadDTO>> GetViolationsByStudentIDAsync(string studentId);
        Task<IEnumerable<ViolationReadDTO>> GetViolationsByRoomIDAsync(string roomId);
        Task<IEnumerable<ViolationReadDTO>> GetViolationsByStatusAsync(string status);

        // Transactions
        Task<string> AddViolationAsync(ViolationCreateDTO dto);
        Task UpdateViolationAsync(string id, ViolationUpdateDTO dto);
        Task DeleteViolationAsync(string id);

        // Student View (Mapping sang GridDTO)
        Task<IEnumerable<ViolationGridDTO>> GetViolationsWithFilterAsync(string? status, string? studentId);

        // Admin View (Mapping sang AdminDTO)
        Task<IEnumerable<ViolationAdminDTO>> GetViolationsForAdminAsync(string? search, string? status, string? roomId);
    }
}