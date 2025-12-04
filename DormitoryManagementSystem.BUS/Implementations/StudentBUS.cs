using AutoMapper;
using DormitoryManagementSystem.BUS.Interfaces;
using DormitoryManagementSystem.DAO.Interfaces;
using DormitoryManagementSystem.DTO.Students;
using DormitoryManagementSystem.DTO.Utils; // Using AppConstants
using DormitoryManagementSystem.Entity;

namespace DormitoryManagementSystem.BUS.Implementations
{
    public class StudentBUS : IStudentBUS
    {
        private readonly IStudentDAO _studentDAO;
        private readonly IContractDAO _contractDAO;
        private readonly IPaymentDAO _paymentDAO;
        private readonly IMapper _mapper;
        private readonly IUserDAO _userDAO;

        public StudentBUS(IStudentDAO studentDAO, IContractDAO contractDAO, IPaymentDAO paymentDAO, IUserDAO userDAO, IMapper mapper)
        {
            _studentDAO = studentDAO;
            _contractDAO = contractDAO;
            _paymentDAO = paymentDAO;
            _userDAO = userDAO;
            _mapper = mapper;
        }

        public async Task<IEnumerable<StudentReadDTO>> GetAllStudentsAsync() =>
            _mapper.Map<IEnumerable<StudentReadDTO>>(await _studentDAO.GetAllStudentsAsync());

        public async Task<IEnumerable<StudentReadDTO>> GetAllStudentsIncludingInactivesAsync() =>
            _mapper.Map<IEnumerable<StudentReadDTO>>(await _studentDAO.GetAllStudentsIncludingInactivesAsync());

        public async Task<StudentReadDTO?> GetStudentByIDAsync(string id)
        {
            var student = await _studentDAO.GetStudentByIDAsync(id);
            return student == null ? null : _mapper.Map<StudentReadDTO>(student);
        }

        public async Task<StudentReadDTO?> GetStudentByCCCDAsync(string cccd)
        {
            var student = await _studentDAO.GetStudentByCCCDAsync(cccd);
            return student == null ? null : _mapper.Map<StudentReadDTO>(student);
        }

        public async Task<StudentReadDTO?> GetStudentByEmailAsync(string email)
        {
            var student = await _studentDAO.GetStudentByEmailAsync(email);
            return student == null ? null : _mapper.Map<StudentReadDTO>(student);
        }

        public async Task<string> AddStudentAsync(StudentCreateDTO dto)
        {
            if (await _studentDAO.GetStudentByIDAsync(dto.StudentID) != null)
                throw new InvalidOperationException($"Student ID {dto.StudentID} đã tồn tại.");
            if (await _studentDAO.GetStudentByCCCDAsync(dto.CCCD) != null)
                throw new InvalidOperationException($"CCCD {dto.CCCD} đã tồn tại.");
            if (!string.IsNullOrEmpty(dto.Email) && await _studentDAO.GetStudentByEmailAsync(dto.Email) != null)
                throw new InvalidOperationException($"Email {dto.Email} đã tồn tại.");

            var student = _mapper.Map<Student>(dto);
            await _studentDAO.AddStudentAsync(student);
            return student.Studentid;
        }

        public async Task UpdateStudentAsync(string id, StudentUpdateDTO dto)
        {
            var student = await _studentDAO.GetStudentByIDAsync(id)
                          ?? throw new KeyNotFoundException($"Student {id} không tìm thấy.");

            if (dto.CCCD != student.Idcard && await _studentDAO.GetStudentByCCCDAsync(dto.CCCD) != null)
                throw new InvalidOperationException($"CCCD {dto.CCCD} đã được sử dụng.");

            if (!string.IsNullOrEmpty(dto.Email) && dto.Email != student.Email && await _studentDAO.GetStudentByEmailAsync(dto.Email) != null)
                throw new InvalidOperationException($"Email {dto.Email} đã được sử dụng.");

            _mapper.Map(dto, student);
            student.Studentid = id;
            await _studentDAO.UpdateStudentAsync(student);
        }

        public async Task DeleteStudentAsync(string id)
        {
            var student = await _studentDAO.GetStudentByIDAsync(id)
                          ?? throw new KeyNotFoundException($"Student {id} không tìm thấy.");

            var activeContract = await _contractDAO.GetActiveContractByStudentIDAsync(id);
            if (activeContract != null)
                throw new InvalidOperationException($"Sinh viên này đang có hợp đồng tại phòng {activeContract.Roomid}. Vui lòng hủy hợp đồng trước.");

            // Soft Delete User (sẽ ẩn luôn Student)
            await _userDAO.DeleteUserAsync(student.Userid);
        }

        public async Task<StudentProfileDTO?> GetStudentProfileAsync(string studentId)
        {
            var student = await _studentDAO.GetStudentByIDAsync(studentId);
            if (student == null) return null;

            var profile = new StudentProfileDTO
            {
                StudentID = student.Studentid,
                FullName = student.Fullname,
                Major = student.Major ?? "N/A",
                DateOfBirth = student.Dateofbirth?.ToString("dd/MM/yyyy") ?? "N/A",
                PhoneNumber = student.Phonenumber,
                Gender = student.Gender ?? "N/A",
                Email = student.Email ?? "N/A",
                CCCD = student.Idcard,
                Address = student.Address ?? "N/A"
            };

            var activeContract = await _contractDAO.GetContractDetailAsync(studentId);
            if (activeContract != null)
            {
                profile.RoomName = activeContract.Room?.Roomnumber.ToString() ?? "Unknown";
                profile.BuildingName = activeContract.Room?.Building?.Buildingname ?? "Unknown";
                profile.ContractStatus = activeContract.Status ?? "N/A";

                var payments = await _paymentDAO.GetPaymentsByContractIDAsync(activeContract.Contractid);
                var debtPayments = payments.Where(p => p.Paymentstatus == AppConstants.PaymentStatus.Unpaid
                                                    || p.Paymentstatus == AppConstants.PaymentStatus.Late).ToList();

                profile.TotalDebt = debtPayments.Sum(p => p.Paymentamount - p.Paidamount);
                profile.AmountToPay = profile.TotalDebt;
                profile.IsDebt = profile.TotalDebt > 0;

                if (profile.IsDebt) profile.PaymentStatusDisplay = "Chưa thanh toán";
                else profile.PaymentStatusDisplay = payments.Any() ? "Đã thanh toán" : "Chưa có hóa đơn";
            }
            else
            {
                profile.RoomName = "Chưa đăng ký";
                profile.PaymentStatusDisplay = "Chưa đăng ký phòng";
            }
            return profile;
        }

        public async Task UpdateContactInfoAsync(string studentId, StudentContactUpdateDTO dto)
        {
            var student = await _studentDAO.GetStudentByIDAsync(studentId)
                          ?? throw new KeyNotFoundException($"Student {studentId} không tìm thấy.");

            if (!string.Equals(student.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await _studentDAO.GetStudentByEmailAsync(dto.Email) != null)
                    throw new InvalidOperationException($"Email '{dto.Email}' đã được sử dụng.");
            }

            student.Phonenumber = dto.PhoneNumber;
            student.Email = dto.Email;
            student.Address = dto.Address;
            await _studentDAO.UpdateStudentAsync(student);
        }
    }
}