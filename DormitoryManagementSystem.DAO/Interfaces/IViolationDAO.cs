using DormitoryManagementSystem.Entity;
using DormitoryManagementSystem.DTO.SearchCriteria; // ViolationSearchCriteria

namespace DormitoryManagementSystem.DAO.Interfaces
{
    public interface IViolationDAO
    {
        // CRUD
        Task<Violation?> GetViolationByIdAsync(string id);
        Task AddNewViolationAsync(Violation violation);
        Task UpdateViolationAsync(Violation violation);
        Task DeleteViolationAsync(string id);

        // MAIN SEARCH FUNCTION
        Task<IEnumerable<Violation>> SearchViolationsAsync(ViolationSearchCriteria criteria);
    }
}