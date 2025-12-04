using DormitoryManagementSystem.BUS.Interfaces;
using DormitoryManagementSystem.DTO.Payments;
using DormitoryManagementSystem.Utils; // AppConstants
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DormitoryManagementSystem.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentBUS _paymentBUS;
        public PaymentController(IPaymentBUS paymentBUS) => _paymentBUS = paymentBUS;

        // ======================== STUDENT API ========================

        [HttpGet("student/payments")]
        [Authorize(Roles = AppConstants.Role.Student)]
        public async Task<IActionResult> GetMyPayments([FromQuery] string? status)
        {
            var studentId = User.FindFirst("StudentID")?.Value;
            if (string.IsNullOrEmpty(studentId)) throw new UnauthorizedAccessException("Token lỗi: Không tìm thấy StudentID.");

            // Nếu status rỗng hoặc "All" -> gửi null để BUS xử lý lấy tất cả
            if (string.IsNullOrEmpty(status) || status.ToLower() == "all") status = null;

            var list = await _paymentBUS.GetPaymentsByStudentAndStatusAsync(studentId, status);
            return Ok(list);
        }

        // ======================== ADMIN API ========================

        [HttpGet("admin/payments")]
        [Authorize(Roles = AppConstants.Role.Admin)]
        public async Task<IActionResult> GetAdminPaymentList(
            [FromQuery] int? month,
            [FromQuery] string? status,
            [FromQuery] string? building,
            [FromQuery] string? search)
        {
            var list = await _paymentBUS.GetPaymentsForAdminAsync(month, status, building, search);
            return Ok(list);
        }

        // Xác nhận đã thanh toán (nghiệp vụ quan trọng)
        [HttpPut("admin/payments/{id}/confirm")]
        [Authorize(Roles = AppConstants.Role.Admin)]
        public async Task<IActionResult> ConfirmPayment(string id, [FromBody] PaymentConfirmDTO dto)
        {
            await _paymentBUS.ConfirmPaymentAsync(id, dto);
            return Ok(new { message = "Ghi nhận thanh toán thành công!" });
        }

        // Tạo hóa đơn thủ công (ít dùng, thường dùng tự động)
        [HttpPost("admin/payments")]
        [Authorize(Roles = AppConstants.Role.Admin)]
        public async Task<IActionResult> CreateBill([FromBody] PaymentCreateDTO dto)
        {
            var id = await _paymentBUS.AddPaymentAsync(dto);
            return Ok(new { message = "Tạo hóa đơn thành công", id });
        }

        // Xóa hóa đơn (nếu tạo sai)
        [HttpDelete("admin/payments/{id}")]
        [Authorize(Roles = AppConstants.Role.Admin)]
        public async Task<IActionResult> DeleteBill(string id)
        {
            await _paymentBUS.DeletePaymentAsync(id);
            return Ok(new { message = "Đã xóa hóa đơn" });
        }

        // Chức năng "Chốt sổ" hàng tháng
        [HttpPost("admin/payments/auto-generate")]
        [Authorize(Roles = AppConstants.Role.Admin)]
        public async Task<IActionResult> GenerateBills([FromQuery] int month, [FromQuery] int year)
        {
            if (year == 0) year = DateTime.Now.Year;
            int count = await _paymentBUS.GenerateMonthlyBillsAsync(month, year);
            return Ok(new { message = $"Đã tạo thành công {count} hóa đơn tiền phòng cho tháng {month}/{year}!" });
        }
    }
}