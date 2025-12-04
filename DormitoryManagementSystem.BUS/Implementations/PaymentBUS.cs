using AutoMapper;
using DormitoryManagementSystem.BUS.Interfaces;
using DormitoryManagementSystem.DAO.Interfaces;
using DormitoryManagementSystem.DTO.Payments;
using DormitoryManagementSystem.DTO.SearchCriteria; // PaymentSearchCriteria
using DormitoryManagementSystem.Utils; // AppConstants
using DormitoryManagementSystem.Entity;

namespace DormitoryManagementSystem.BUS.Implementations
{
    public class PaymentBUS : IPaymentBUS
    {
        private readonly IPaymentDAO _paymentDAO;
        private readonly IContractDAO _contractDAO;
        private readonly IRoomDAO _roomDAO;
        private readonly IMapper _mapper;

        public PaymentBUS(IPaymentDAO paymentDAO, IContractDAO contractDAO, IMapper mapper, IRoomDAO roomDAO)
        {
            _paymentDAO = paymentDAO;
            _contractDAO = contractDAO;
            _mapper = mapper;
            _roomDAO = roomDAO;
        }

        // ======================== GET & SEARCH ========================

        public async Task<IEnumerable<PaymentReadDTO>> GetAllPaymentsAsync()
        {
            var list = await _paymentDAO.SearchPaymentsAsync(new PaymentSearchCriteria());
            return _mapper.Map<IEnumerable<PaymentReadDTO>>(list);
        }

        public async Task<PaymentReadDTO?> GetPaymentByIDAsync(string id)
        {
            var p = await _paymentDAO.GetPaymentByIDAsync(id);
            return p == null ? null : _mapper.Map<PaymentReadDTO>(p);
        }

        public async Task<IEnumerable<PaymentReadDTO>> GetPaymentsByContractIDAsync(string contractId)
        {
            if (await _contractDAO.GetContractByIDAsync(contractId) == null)
                throw new KeyNotFoundException($"Hợp đồng {contractId} không tồn tại.");

            var list = await _paymentDAO.SearchPaymentsAsync(new PaymentSearchCriteria { ContractID = contractId });
            return _mapper.Map<IEnumerable<PaymentReadDTO>>(list);
        }

        public async Task<IEnumerable<PaymentReadDTO>> GetPaymentsByStatusAsync(string status)
        {
            var list = await _paymentDAO.SearchPaymentsAsync(new PaymentSearchCriteria { Status = status });
            return _mapper.Map<IEnumerable<PaymentReadDTO>>(list);
        }

        // Student View
        public async Task<IEnumerable<PaymentListDTO>> GetPaymentsByStudentAndStatusAsync(string studentId, string? status)
        {
            var criteria = new PaymentSearchCriteria { StudentID = studentId, Status = status };
            var list = await _paymentDAO.SearchPaymentsAsync(criteria);

            return list.Select(p => new PaymentListDTO
            {
                PaymentID = p.Paymentid,
                BillMonth = p.Billmonth,
                PaymentAmount = p.Paymentamount,
                PaymentStatus = p.Paymentstatus ?? "Unknown",
                Description = p.Description ?? "",
                PaymentDate = p.Paymentdate
            });
        }

        // Admin View
        public async Task<IEnumerable<PaymentAdminDTO>> GetPaymentsForAdminAsync(int? month, string? status, string? building, string? search)
        {
            var criteria = new PaymentSearchCriteria
            {
                Month = month,
                Status = status,
                BuildingID = building,
                Keyword = search
            };

            var list = await _paymentDAO.SearchPaymentsAsync(criteria);

            return list.Select(p => new PaymentAdminDTO
            {
                PaymentID = p.Paymentid,
                ContractID = p.Contractid,
                StudentID = p.Contract?.Studentid ?? "",
                StudentName = p.Contract?.Student?.Fullname ?? "Unknown",
                RoomName = p.Contract?.Room?.Roomnumber.ToString() ?? "N/A",
                BillMonth = p.Billmonth,
                PaymentAmount = p.Paymentamount,
                PaymentStatus = p.Paymentstatus ?? "Unpaid",
                PaymentDate = p.Paymentdate,
                PaymentMethod = p.Paymentmethod ?? ""
            });
        }

        // ======================== TRANSACTIONS ========================

        public async Task<string> AddPaymentAsync(PaymentCreateDTO dto)
        {
            if (await _paymentDAO.GetPaymentByIDAsync(dto.PaymentID) != null)
                throw new InvalidOperationException($"Hóa đơn {dto.PaymentID} đã tồn tại.");

            if (await _contractDAO.GetContractByIDAsync(dto.ContractID) == null)
                throw new KeyNotFoundException($"Hợp đồng {dto.ContractID} không tồn tại.");

            if (dto.PaidAmount > dto.PaymentAmount)
                throw new InvalidOperationException("Số tiền đóng không được lớn hơn số tiền phải thu.");

            // Tự động set Paid nếu đóng đủ
            if (dto.PaidAmount >= dto.PaymentAmount && dto.PaymentStatus == AppConstants.PaymentStatus.Unpaid)
                dto.PaymentStatus = AppConstants.PaymentStatus.Paid;

            var payment = _mapper.Map<Payment>(dto);
            if (payment.Paymentdate == null) payment.Paymentdate = DateTime.Now;

            await _paymentDAO.AddPaymentAsync(payment);
            return payment.Paymentid;
        }

        public async Task UpdatePaymentAsync(string id, PaymentUpdateDTO dto)
        {
            var payment = await _paymentDAO.GetPaymentByIDAsync(id)
                          ?? throw new KeyNotFoundException($"Hóa đơn {id} không tồn tại.");

            if (dto.PaidAmount > payment.Paymentamount)
                throw new InvalidOperationException("Số tiền đóng vượt quá số tiền gốc.");

            _mapper.Map(dto, payment);
            payment.Paymentid = id;

            // Logic tự động cập nhật trạng thái
            if (payment.Paidamount >= payment.Paymentamount && payment.Paymentstatus != AppConstants.PaymentStatus.Paid)
                payment.Paymentstatus = AppConstants.PaymentStatus.Paid;

            await _paymentDAO.UpdatePaymentAsync(payment);
        }

        public async Task DeletePaymentAsync(string id)
        {
            if (await _paymentDAO.GetPaymentByIDAsync(id) == null)
                throw new KeyNotFoundException($"Hóa đơn {id} không tồn tại.");
            await _paymentDAO.RemovePaymentAsync(id);
        }

        public async Task ConfirmPaymentAsync(string id, PaymentConfirmDTO dto)
        {
            var payment = await _paymentDAO.GetPaymentByIDAsync(id)
                          ?? throw new KeyNotFoundException($"Hóa đơn {id} không tồn tại.");

            if (payment.Paymentstatus == AppConstants.PaymentStatus.Paid)
                throw new InvalidOperationException("Hóa đơn này đã thanh toán rồi.");

            payment.Paymentstatus = AppConstants.PaymentStatus.Paid;
            payment.Paymentdate = DateTime.Now;
            payment.Paymentmethod = dto.PaymentMethod;

            if (!string.IsNullOrEmpty(dto.Note))
                payment.Description = string.IsNullOrEmpty(payment.Description) ? dto.Note : $"{payment.Description} | Note: {dto.Note}";

            // Auto-fill amount
            if (payment.Paidamount < payment.Paymentamount) payment.Paidamount = payment.Paymentamount;

            await _paymentDAO.UpdatePaymentAsync(payment);
        }

        public async Task<int> GenerateMonthlyBillsAsync(int month, int year)
        {
            // Lấy Hợp đồng Active
            var activeContracts = await _contractDAO.SearchContractsAsync(new ContractSearchCriteria { Status = AppConstants.ContractStatus.Active });
            int count = 0;

            foreach (var contract in activeContracts)
            {
                // Check đã có bill tháng này chưa
                var existing = await _paymentDAO.SearchPaymentsAsync(new PaymentSearchCriteria
                {
                    ContractID = contract.Contractid,
                    Month = month,
                    Year = year
                });

                if (existing.Any()) continue;

                var room = await _roomDAO.GetRoomByIDAsync(contract.Roomid);
                var newBill = new PaymentCreateDTO
                {
                    PaymentID = $"PAY_{year}{month}_{contract.Contractid}",
                    ContractID = contract.Contractid,
                    BillMonth = month,
                    PaymentAmount = room?.Price ?? 0,
                    PaymentStatus = AppConstants.PaymentStatus.Unpaid,
                    Description = $"Tiền phòng tháng {month}/{year}",
                    PaymentDate = null
                };

                await AddPaymentAsync(newBill);
                count++;
            }
            return count;
        }
    }
}