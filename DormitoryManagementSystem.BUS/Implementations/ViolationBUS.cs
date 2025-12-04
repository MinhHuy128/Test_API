using AutoMapper;
using DormitoryManagementSystem.BUS.Interfaces;
using DormitoryManagementSystem.DAO.Interfaces;
using DormitoryManagementSystem.DTO.SearchCriteria; // Criteria
using DormitoryManagementSystem.DTO.Utils; // AppConstants
using DormitoryManagementSystem.DTO.Violations;
using DormitoryManagementSystem.Entity;

namespace DormitoryManagementSystem.BUS.Implementations
{
    public class ViolationBUS : IViolationBUS
    {
        private readonly IViolationDAO _violationDAO;
        private readonly IRoomDAO _roomDAO;
        private readonly IStudentDAO _studentDAO;
        private readonly IUserDAO _userDAO;
        private readonly IMapper _mapper;

        public ViolationBUS(IViolationDAO violationDAO, IRoomDAO roomDAO, IStudentDAO studentDAO, IUserDAO userDAO, IMapper mapper)
        {
            _violationDAO = violationDAO;
            _roomDAO = roomDAO;
            _studentDAO = studentDAO;
            _userDAO = userDAO;
            _mapper = mapper;
        }

        // ======================== GET & SEARCH ========================

        public async Task<IEnumerable<ViolationReadDTO>> GetAllViolationsAsync()
        {
            var list = await _violationDAO.SearchViolationsAsync(new ViolationSearchCriteria());
            return _mapper.Map<IEnumerable<ViolationReadDTO>>(list);
        }

        public async Task<ViolationReadDTO?> GetViolationByIDAsync(string id)
        {
            var v = await _violationDAO.GetViolationByIdAsync(id);
            return v == null ? null : _mapper.Map<ViolationReadDTO>(v);
        }

        public async Task<IEnumerable<ViolationReadDTO>> GetViolationsByStudentIDAsync(string sId)
        {
            var list = await _violationDAO.SearchViolationsAsync(new ViolationSearchCriteria { StudentID = sId });
            return _mapper.Map<IEnumerable<ViolationReadDTO>>(list);
        }

        public async Task<IEnumerable<ViolationReadDTO>> GetViolationsByRoomIDAsync(string rId)
        {
            if (await _roomDAO.GetRoomByIDAsync(rId) == null)
                throw new KeyNotFoundException($"Phòng {rId} không tồn tại.");

            var list = await _violationDAO.SearchViolationsAsync(new ViolationSearchCriteria { RoomID = rId });
            return _mapper.Map<IEnumerable<ViolationReadDTO>>(list);
        }

        public async Task<IEnumerable<ViolationReadDTO>> GetViolationsByStatusAsync(string status)
        {
            var list = await _violationDAO.SearchViolationsAsync(new ViolationSearchCriteria { Status = status });
            return _mapper.Map<IEnumerable<ViolationReadDTO>>(list);
        }

        // Student View
        public async Task<IEnumerable<ViolationGridDTO>> GetViolationsWithFilterAsync(string? status, string? sId)
        {
            var criteria = new ViolationSearchCriteria { Status = status, StudentID = sId };
            var list = await _violationDAO.SearchViolationsAsync(criteria);

            return list.Select(v => new ViolationGridDTO
            {
                ViolationID = v.Violationid,
                RoomNumber = v.Room?.Roomnumber ?? 0,
                Status = v.Status ?? "Unknown",
                StartTime = v.Violationdate ?? DateTime.MinValue,
                ViolationType = v.Violationtype,
                PenaltyFee = v.Penaltyfee ?? 0
            });
        }

        // Admin View
        public async Task<IEnumerable<ViolationAdminDTO>> GetViolationsForAdminAsync(string? search, string? status, string? roomId)
        {
            var criteria = new ViolationSearchCriteria { Keyword = search, Status = status, RoomID = roomId };
            var list = await _violationDAO.SearchViolationsAsync(criteria);

            return list.Select(v => new ViolationAdminDTO
            {
                ViolationID = v.Violationid,
                StudentID = v.Studentid,
                StudentName = v.Student?.Fullname ?? "Unknown",
                RoomID = v.Roomid,
                ViolationType = v.Violationtype,
                ViolationDate = v.Violationdate ?? DateTime.Now,
                PenaltyFee = v.Penaltyfee ?? 0,
                Status = v.Status
            });
        }

        // ======================== TRANSACTIONS ========================

        public async Task<string> AddViolationAsync(ViolationCreateDTO dto)
        {
            if (await _violationDAO.GetViolationByIdAsync(dto.ViolationID) != null)
                throw new InvalidOperationException($"Mã vi phạm {dto.ViolationID} đã tồn tại.");

            if (await _roomDAO.GetRoomByIDAsync(dto.RoomID) == null)
                throw new KeyNotFoundException($"Phòng {dto.RoomID} không tồn tại.");

            if (!string.IsNullOrEmpty(dto.StudentID))
            {
                if (await _studentDAO.GetStudentByIDAsync(dto.StudentID) == null)
                    throw new KeyNotFoundException($"Sinh viên {dto.StudentID} không tồn tại.");
            }

            if (!string.IsNullOrEmpty(dto.ReportedByUserID))
            {
                var reporter = await _userDAO.GetUserByIDAsync(dto.ReportedByUserID);
                if (reporter == null || !reporter.IsActive)
                    throw new InvalidOperationException($"Người báo cáo {dto.ReportedByUserID} không hợp lệ.");
            }

            var violation = _mapper.Map<Violation>(dto);
            if (violation.Violationdate == null) violation.Violationdate = DateTime.Now;

            // Set Default Status if needed
            if (string.IsNullOrEmpty(violation.Status)) violation.Status = AppConstants.ViolationStatus.Pending;

            await _violationDAO.AddNewViolationAsync(violation);
            return violation.Violationid;
        }

        public async Task UpdateViolationAsync(string id, ViolationUpdateDTO dto)
        {
            var violation = await _violationDAO.GetViolationByIdAsync(id)
                            ?? throw new KeyNotFoundException($"Vi phạm {id} không tồn tại.");

            // Nếu cập nhật sinh viên vi phạm, phải check tồn tại
            if (!string.IsNullOrEmpty(dto.StudentID) && dto.StudentID != violation.Studentid)
            {
                if (await _studentDAO.GetStudentByIDAsync(dto.StudentID) == null)
                    throw new KeyNotFoundException($"Sinh viên {dto.StudentID} không tồn tại.");
            }

            _mapper.Map(dto, violation);
            violation.Violationid = id; // Bảo vệ ID
            await _violationDAO.UpdateViolationAsync(violation);
        }

        public async Task DeleteViolationAsync(string id)
        {
            if (await _violationDAO.GetViolationByIdAsync(id) == null)
                throw new KeyNotFoundException($"Vi phạm {id} không tồn tại.");

            await _violationDAO.DeleteViolationAsync(id);
        }
    }
}