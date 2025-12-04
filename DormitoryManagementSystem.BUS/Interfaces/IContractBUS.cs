using DormitoryManagementSystem.DTO.Contracts;

namespace DormitoryManagementSystem.BUS.Interfaces
{
    public interface IContractBUS
    {
        // Basic Read
        Task<IEnumerable<ContractReadDTO>> GetAllContractsAsync();
        Task<ContractReadDTO?> GetContractByIDAsync(string id);

        // Search & Filter (Gọi SearchDAO)
        Task<IEnumerable<ContractReadDTO>> GetContractsByStudentIDAsync(string studentId);
        Task<IEnumerable<ContractReadDTO>> GetContractsAsync(string searchTerm);
        Task<IEnumerable<ContractReadDTO>> GetContractsByMultiConditionAsync(ContractFilterDTO filter);

        // Student Specific
        Task<ContractDetailDTO?> GetContractFullDetailAsync(string studentId);
        Task<string> RegisterContractAsync(string studentId, ContractRegisterDTO dto);

        // Transactions
        Task<string> AddContractAsync(ContractCreateDTO dto, string staffUserID);
        Task UpdateContractAsync(string id, ContractUpdateDTO dto);
        Task DeleteContractAsync(string id);
    }
}