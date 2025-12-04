using DormitoryManagementSystem.DTO.Payments;

namespace DormitoryManagementSystem.BUS.Interfaces
{
    public interface IPaymentBUS
    {
        // Basic Read
        Task<IEnumerable<PaymentReadDTO>> GetAllPaymentsAsync();
        Task<PaymentReadDTO?> GetPaymentByIDAsync(string id);

        // Search Wrappers (Sẽ gọi SearchDAO bên trong)
        Task<IEnumerable<PaymentReadDTO>> GetPaymentsByContractIDAsync(string contractId);
        Task<IEnumerable<PaymentReadDTO>> GetPaymentsByStatusAsync(string status);

        // Transactions
        Task<string> AddPaymentAsync(PaymentCreateDTO dto);
        Task UpdatePaymentAsync(string id, PaymentUpdateDTO dto);
        Task DeletePaymentAsync(string id);

        // Business Logic Methods
        Task ConfirmPaymentAsync(string id, PaymentConfirmDTO dto);
        Task<int> GenerateMonthlyBillsAsync(int month, int year);

        // View Specific Methods (Mapping sang DTO đặc thù)
        Task<IEnumerable<PaymentListDTO>> GetPaymentsByStudentAndStatusAsync(string studentId, string? status);
        Task<IEnumerable<PaymentAdminDTO>> GetPaymentsForAdminAsync(int? month, string? status, string? building, string? search);
    }
}