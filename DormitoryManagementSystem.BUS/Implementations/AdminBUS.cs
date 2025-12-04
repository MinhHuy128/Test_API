using AutoMapper;
using DormitoryManagementSystem.BUS.Interfaces;
using DormitoryManagementSystem.DAO.Interfaces;
using DormitoryManagementSystem.DTO.Admins;
using DormitoryManagementSystem.Entity;
using DormitoryManagementSystem.Utils; // Using AppConstants

namespace DormitoryManagementSystem.BUS.Implementations
{
    public class AdminBUS : IAdminBUS
    {
        private readonly IAdminDAO _adminDAO;
        private readonly IUserDAO _userDAO;
        private readonly IMapper _mapper;

        public AdminBUS(IAdminDAO adminDAO, IUserDAO userDAO, IMapper mapper)
        {
            _adminDAO = adminDAO;
            _userDAO = userDAO;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AdminReadDTO>> GetAllAdminsAsync() =>
            _mapper.Map<IEnumerable<AdminReadDTO>>(await _adminDAO.GetAllAdminsAsync());

        public async Task<IEnumerable<AdminReadDTO>> GetAllAdminsIncludingInactivesAsync() =>
            _mapper.Map<IEnumerable<AdminReadDTO>>(await _adminDAO.GetAllAdminsIncludingInactivesAsync());

        public async Task<AdminReadDTO?> GetAdminByIDAsync(string id)
        {
            var ad = await _adminDAO.GetAdminByIDAsync(id);
            return ad == null ? null : _mapper.Map<AdminReadDTO>(ad);
        }

        public async Task<AdminReadDTO?> GetAdminByUserIDAsync(string userId)
        {
            var ad = await _adminDAO.GetAdminByUserIDAsync(userId);
            return ad == null ? null : _mapper.Map<AdminReadDTO>(ad);
        }

        public async Task<string> AddAdminAsync(AdminCreateDTO dto)
        {
            if (await _adminDAO.GetAdminByIDAsync(dto.AdminID) != null)
                throw new InvalidOperationException($"Admin ID {dto.AdminID} đã tồn tại.");

            if (await _adminDAO.GetAdminByCCCDAsync(dto.IDcard) != null)
                throw new InvalidOperationException($"CCCD {dto.IDcard} đã tồn tại.");

            var user = await _userDAO.GetUserByIDAsync(dto.UserID)
                       ?? throw new KeyNotFoundException($"User {dto.UserID} không tồn tại.");

            if (user.Role != AppConstants.Role.Admin)
                throw new InvalidOperationException($"User {dto.UserID} không phải là Admin.");

            if (await _adminDAO.GetAdminByUserIDAsync(dto.UserID) != null)
                throw new InvalidOperationException($"User {dto.UserID} đã có hồ sơ Admin.");

            var ad = _mapper.Map<Admin>(dto);
            await _adminDAO.AddAdminAsync(ad);
            return ad.Adminid;
        }

        public async Task UpdateAdminAsync(string id, AdminUpdateDTO dto)
        {
            var currentAdmin = await _adminDAO.GetAdminByIDAsync(id)
                               ?? throw new KeyNotFoundException($"Admin {id} không tồn tại.");

            if (currentAdmin.Idcard != dto.IDcard && await _adminDAO.GetAdminByCCCDAsync(dto.IDcard) != null)
                throw new InvalidOperationException($"CCCD {dto.IDcard} đã được sử dụng.");

            _mapper.Map(dto, currentAdmin);
            currentAdmin.Adminid = id;
            await _adminDAO.UpdateAdminAsync(currentAdmin);
        }

        public async Task DeleteAdminAsync(string id)
        {
            var existing = await _adminDAO.GetAdminByIDAsync(id)
                           ?? throw new KeyNotFoundException($"Admin {id} không tồn tại.");

            // Soft Delete User liên kết
            await _userDAO.DeleteUserAsync(existing.Userid);
        }
    }
}