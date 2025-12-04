using DormitoryManagementSystem.BUS.Interfaces;
using DormitoryManagementSystem.DTO.Contracts;
using DormitoryManagementSystem.Utils; // AppConstants
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DormitoryManagementSystem.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class ContractController : ControllerBase
    {
        private readonly IContractBUS _contractBUS;
        public ContractController(IContractBUS contractBUS) => _contractBUS = contractBUS;

        // --- STUDENT API ---

        [HttpGet("student/my-contracts")]
        [Authorize(Roles = AppConstants.Role.Student)]
        public async Task<IActionResult> GetMyContracts()
        {
            var studentId = User.FindFirst("StudentID")?.Value;
            if (string.IsNullOrEmpty(studentId)) throw new UnauthorizedAccessException("Không xác định được sinh viên.");

            var contract = await _contractBUS.GetContractFullDetailAsync(studentId);
            // Trả về null nếu chưa có HĐ cũng ok, FE tự xử lý
            return Ok(contract);
        }

        [HttpPost("student/contracts")]
        [Authorize(Roles = AppConstants.Role.Student)]
        public async Task<IActionResult> RegisterContract([FromBody] ContractRegisterDTO dto)
        {
            var studentId = User.FindFirst("StudentID")?.Value;
            if (string.IsNullOrEmpty(studentId)) throw new UnauthorizedAccessException("Không xác định được sinh viên.");

            var contractId = await _contractBUS.RegisterContractAsync(studentId, dto);
            return Ok(new { message = "Gửi đơn đăng ký thành công!", contractId });
        }

        // --- ADMIN API ---

        [HttpGet("admin/contracts")]
        [Authorize(Roles = AppConstants.Role.Admin)]
        public async Task<IActionResult> GetContracts([FromQuery] string? searchTerm)
        {
            var result = await _contractBUS.GetContractsAsync(searchTerm);
            return Ok(result);
        }

        [HttpGet("admin/contracts/filter")]
        [Authorize(Roles = AppConstants.Role.Admin)]
        public async Task<IActionResult> GetContractsByMultiCondition([FromQuery] ContractFilterDTO filter)
        {
            var result = await _contractBUS.GetContractsByMultiConditionAsync(filter);
            return Ok(result);
        }

        [HttpGet("admin/contracts/{id}")]
        [Authorize(Roles = AppConstants.Role.Admin)]
        public async Task<IActionResult> GetContractById(string id)
        {
            var contract = await _contractBUS.GetContractByIDAsync(id);
            if (contract == null) throw new KeyNotFoundException("Không tìm thấy hợp đồng.");
            return Ok(contract);
        }

        // Tạo hợp đồng thủ công (nếu cần)
        [HttpPost("admin/contracts")]
        [Authorize(Roles = AppConstants.Role.Admin)]
        public async Task<IActionResult> CreateContract([FromBody] ContractCreateDTO dto)
        {
            var staffId = User.FindFirst("UserID")?.Value;
            await _contractBUS.AddContractAsync(dto, staffId ?? "Unknown");
            return Ok(new { message = "Tạo hợp đồng thành công!" });
        }

        [HttpPut("admin/contracts/{id}")]
        [Authorize(Roles = AppConstants.Role.Admin)]
        public async Task<IActionResult> UpdateContract(string id, [FromBody] ContractUpdateDTO dto)
        {
            await _contractBUS.UpdateContractAsync(id, dto);
            return Ok(new { message = "Cập nhật hợp đồng thành công!" });
        }

        [HttpDelete("admin/contracts/{id}")]
        [Authorize(Roles = AppConstants.Role.Admin)]
        public async Task<IActionResult> DeleteContract(string id)
        {
            await _contractBUS.DeleteContractAsync(id);
            return Ok(new { message = "Xóa hợp đồng thành công!" });
        }
    }
}