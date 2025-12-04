using DormitoryManagementSystem.BUS.Interfaces;
using DormitoryManagementSystem.DTO.Admins;
using DormitoryManagementSystem.DTO.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DormitoryManagementSystem.API.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminBUS _adminBUS;

        public AdminController(IAdminBUS adminBUS)
        {
            _adminBUS = adminBUS;
        }

        [HttpGet("admins")]

        public async Task<ActionResult<IEnumerable<AdminReadDTO>>> GetAllAdmins()
        {
            var admins = await _adminBUS.GetAllAdminsAsync();
            return Ok(admins.OrderBy(a => a.UserID).ToList());
        }

        [HttpGet("admins/{id}")]
        public async Task<IActionResult> GetAdminById(string id)
        {
            var admin = await _adminBUS.GetAdminByIDAsync(id);
            if (admin == null)
            {
                return NotFound(new { message = $"Không tìm thấy Admin có mã: {id}" });
            }
            return Ok(admin);
        }

        [HttpPost("admins")]
        public async Task<IActionResult> CreateAdmin([FromBody] AdminCreateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var newAdminId = await _adminBUS.AddAdminAsync(dto);
                return StatusCode(201, new { message = "Thêm Admin thành công!", adminId = newAdminId });
            }
            catch (InvalidOperationException ex) 
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }

        }

        [HttpPut("admins/{id}")]
        public async Task<IActionResult> UpdateAdmin(string id, [FromBody] AdminUpdateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _adminBUS.UpdateAdminAsync(id, dto);
                return Ok(new { message = "Cập nhật thông tin Admin thành công!" });
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

        [HttpDelete("admins/{id}")]
        public async Task<IActionResult> DeleteAdmin(string id)
        {
            try
            { 
                await _adminBUS.DeleteAdminAsync(id);
                return Ok(new { message = "Xóa Admin thành công!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) 
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}

