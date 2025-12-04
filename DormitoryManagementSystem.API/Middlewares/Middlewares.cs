using System;
using System.Net;
using System.Threading.Tasks;
using DormitoryManagementSystem.DTO; // Nhớ import DTO
using DormitoryManagementSystem.DTO.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DormitoryManagementSystem.API.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Cho request đi qua bình thường
                await _next(context);
            }
            catch (Exception ex)
            {
                // Nếu có lỗi, nhảy vào đây xử lý
                _logger.LogError(ex, "Đã xảy ra lỗi hệ thống: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // Mặc định là lỗi 500 (Internal Server Error)
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var message = "Đã xảy ra lỗi hệ thống. Vui lòng liên hệ Admin.";

            // Tùy chỉnh thông báo dựa trên loại lỗi (Ví dụ: Lỗi nghiệp vụ, lỗi tìm không thấy...)
            // Bạn có thể mở rộng phần này sau
            if (exception is ArgumentException || exception is InvalidOperationException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest; // 400
                message = exception.Message; // Lỗi do người dùng gửi sai dữ liệu thì báo chi tiết
            }
            else if (exception is KeyNotFoundException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound; // 404
                message = "Không tìm thấy dữ liệu yêu cầu.";
            }

            // Tạo object trả về
            var response = new ErrorResponse
            {
                StatusCode = context.Response.StatusCode,
                Message = message
            };

            return context.Response.WriteAsync(response.ToString());
        }
    }
}