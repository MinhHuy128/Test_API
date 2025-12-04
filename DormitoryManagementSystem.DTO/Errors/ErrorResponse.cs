using System.Text.Json;
namespace DormitoryManagementSystem.DTO.Errors
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;

        // Hàm này giúp chuyển object thành chuỗi JSON
        public override string ToString() => JsonSerializer.Serialize(this);
    }
}
