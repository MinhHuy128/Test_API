using System.Security.Claims; 
using DormitoryManagementSystem.BUS.Interfaces;
using DormitoryManagementSystem.DTO.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DormitoryManagementSystem.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserBUS _userBUS;

        public UserController(IUserBUS userBUS)
        {
            _userBUS = userBUS;
        }

        // PUT: api/user/change-password
        [HttpPut("user/change-password")]
        [Authorize(Roles = "Student")] // tắt cái này để test
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            try
            {
                
                 var userId = User.FindFirst("UserID")?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Token không hợp lệ hoặc đã hết hạn." });

                await _userBUS.ChangePasswordAsync(userId, dto);

                return Ok(new { message = "Đổi mật khẩu thành công!" });
            }
            catch (ArgumentException ex) // Lỗi do người dùng (sai pass cũ, trùng pass cũ)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex) // Lỗi không tìm thấy user
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex) // Lỗi hệ thống khác
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet("user")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers([FromQuery] string? search)
        {
            var users = await _userBUS.GetAllUsersAsync();
            return Ok(users);
        }


        [HttpPost("user")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                
                var newUserId = await _userBUS.AddUserAsync(dto);
                return StatusCode(201, new { message = "Tạo tài khoản thành công!", userId = newUserId });

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                await _userBUS.DeleteUserAsync(id);
                return Ok(new { message = "Xóa tài khoản thành công!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

    }
}
