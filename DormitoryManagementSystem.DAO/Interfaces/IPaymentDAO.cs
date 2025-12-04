using DormitoryManagementSystem.Entity;
using DormitoryManagementSystem.DTO.SearchCriteria; // PaymentSearchCriteria

namespace DormitoryManagementSystem.DAO.Interfaces
{
    public interface IPaymentDAO
    {
        // CRUD
        Task<Payment?> GetPaymentByIDAsync(string id);
        Task AddPaymentAsync(Payment payment);
        Task UpdatePaymentAsync(Payment payment);
        Task RemovePaymentAsync(string id);

        // MAIN SEARCH FUNCTION
        Task<IEnumerable<Payment>> SearchPaymentsAsync(PaymentSearchCriteria criteria);
    }
}