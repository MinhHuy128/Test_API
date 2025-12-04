using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using DormitoryManagementSystem.BUS.Interfaces;
using DormitoryManagementSystem.DAO.Interfaces;
using DormitoryManagementSystem.DTO.Users;
using DormitoryManagementSystem.DTO.Utils; // Nhớ dùng namespace chứa AppConstants bạn mới tạo
using DormitoryManagementSystem.Entity;
using DormitoryManagementSystem.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DormitoryManagementSystem.BUS.Implementations
{
    public class UserBUS : IUserBUS
    {
        private readonly IUserDAO _userDAO;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public UserBUS(IUserDAO userDAO, IMapper mapper, IConfiguration configuration)
        {
            _userDAO = userDAO;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<IEnumerable<UserReadDTO>> GetAllUsersAsync() =>
            _mapper.Map<IEnumerable<UserReadDTO>>(await _userDAO.GetAllUsersAsync());

        public async Task<IEnumerable<UserReadDTO>> GetAllUsersIncludingInactivesAsync() =>
            _mapper.Map<IEnumerable<UserReadDTO>>(await _userDAO.GetAllUsersIncludingInactivesAsync());

        public async Task<UserReadDTO?> GetUserByIDAsync(string id)
        {
            var user = await _userDAO.GetUserByIDAsync(id);
            return user == null ? null : _mapper.Map<UserReadDTO>(user);
        }

        public async Task<User?> GetUserByUsernameAsync(string username) =>
            await _userDAO.GetUserByUsernameAsync(username);

        public async Task<LoginResponseDTO?> LoginAsync(UserLoginDTO dto)
        {
            var user = await _userDAO.GetUserByUsernameAsync(dto.UserName);
            if (user == null || !user.IsActive) return null;
            if (user.Role != dto.Role) return null;

            bool isPasswordValid = false;
            bool isPlainText = !IsBCryptHash(user.Password);

            if (isPlainText)
            {
                // Logic tương thích ngược: Nếu pass chưa hash thì so sánh thường
                if (user.Password == dto.Password)
                {
                    // Tự động cập nhật bảo mật: Hash lại mật khẩu
                    user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                    await _userDAO.UpdateUserAsync(user);
                    isPasswordValid = true;
                }
            }
            else
            {
                try { isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.Password); }
                catch { isPasswordValid = false; }
            }

            if (!isPasswordValid) return null;

            return new LoginResponseDTO
            {
                Token = GenerateJwtToken(user),
                UserID = user.Userid,
                UserName = user.Username,
                Role = user.Role
            };
        }

        public async Task<string> AddUserAsync(UserCreateDTO dto)
        {
            if (await _userDAO.GetUserByIDAsync(dto.UserID) != null)
                throw new InvalidOperationException($"User ID {dto.UserID} đã tồn tại.");

            if (await _userDAO.GetUserByUsernameAsync(dto.UserName) != null)
                throw new InvalidOperationException($"Username '{dto.UserName}' đã được sử dụng.");

            var userEntity = _mapper.Map<User>(dto);
            userEntity.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _userDAO.AddUserAsync(userEntity);
            return userEntity.Userid;
        }

        public async Task UpdateUserAsync(string id, UserUpdateDTO dto)
        {
            var userEntity = await _userDAO.GetUserByIDAsync(id)
                             ?? throw new KeyNotFoundException($"User {id} không tìm thấy.");

            if (!string.IsNullOrEmpty(dto.Role) && dto.Role != AppConstants.Role.Admin && dto.Role != AppConstants.Role.Student)
                throw new InvalidOperationException("Role không hợp lệ.");

            _mapper.Map(dto, userEntity);

            if (!string.IsNullOrEmpty(dto.Password))
                userEntity.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            userEntity.Userid = id;
            await _userDAO.UpdateUserAsync(userEntity);
        }

        public async Task DeleteUserAsync(string id)
        {
            if (await _userDAO.GetUserByIDAsync(id) == null)
                throw new KeyNotFoundException($"User {id} không tìm thấy.");
            await _userDAO.DeleteUserAsync(id);
        }

        public async Task ChangePasswordAsync(string userId, ChangePasswordDTO dto)
        {
            var user = await _userDAO.GetUserByIDAsync(userId)
                       ?? throw new KeyNotFoundException("Không tìm thấy User.");

            if (dto.OldPassword == dto.NewPassword)
                throw new ArgumentException("Mật khẩu mới không được trùng mật khẩu cũ.");

            bool isOldPassCorrect = !IsBCryptHash(user.Password)
                ? user.Password == dto.OldPassword
                : BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.Password);

            if (!isOldPassCorrect) throw new ArgumentException("Mật khẩu hiện tại không chính xác.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _userDAO.UpdateUserAsync(user);
        }

        // --- Helper Methods ---
        private bool IsBCryptHash(string hash) =>
            !string.IsNullOrWhiteSpace(hash) && hash.Length == 60 &&
            (hash.StartsWith("$2a$") || hash.StartsWith("$2b$") || hash.StartsWith("$2y$"));

        private string GenerateJwtToken(User user)
        {
            var secretKey = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim("UserID", user.Userid),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("StudentID", user.Student?.Studentid ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddHours(4),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}