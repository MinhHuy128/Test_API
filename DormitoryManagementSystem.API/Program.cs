using System.Text;
using System.Text.Json;
using AutoMapper;
using DormitoryManagementSystem.API.Middlewares;
using DormitoryManagementSystem.BUS.Implementations;
using DormitoryManagementSystem.BUS.Implements;
using DormitoryManagementSystem.BUS.Interfaces;
using DormitoryManagementSystem.DAO.Context;
using DormitoryManagementSystem.DAO.Implementations;
using DormitoryManagementSystem.DAO.Implements;
using DormitoryManagementSystem.DAO.Interfaces;
using DormitoryManagementSystem.DTO.Errors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- CẤU HÌNH JWT AUTHENTICATION ---
// 1. Lấy cấu hình JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

// Kiểm tra (Safety check)
if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
{
    throw new InvalidOperationException("Chưa cấu hình đầy đủ JWT trong appsettings.json");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

        // BẬT KIỂM TRA LẠI (Vì bây giờ tên đã cố định là "DormitoryAuthServer" ở mọi nơi)
        ValidateIssuer = true,
        ValidIssuer = issuer,

        ValidateAudience = true,
        ValidAudience = audience,

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

options.Events = new JwtBearerEvents
{
    // 1. Xử lý khi chưa đăng nhập hoặc Token sai (Lỗi 401)
    OnChallenge = context =>
    {
        // Bỏ qua phản hồi mặc định (trống trơn)
        context.HandleResponse();

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            StatusCode = 401,
            Message = "Bạn chưa đăng nhập hoặc Token không hợp lệ."
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    },

    // 2. Xử lý khi đăng nhập rồi nhưng không đủ quyền (Lỗi 403)
    OnForbidden = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            StatusCode = 403,
            Message = "Bạn không có quyền truy cập vào chức năng này."
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
});
builder.Services.AddAuthorization();
// --- CẤU HÌNH SWAGGER CHUẨN ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dormitory API", Version = "v1" });

    // Định nghĩa Bearer Token (Đơn giản hóa để tránh lỗi thao tác)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http, // Dùng Http thay vì ApiKey để nó tự hiểu chuẩn Bearer
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập Token vào ô bên dưới (CHỈ DÁN MÃ TOKEN, KHÔNG CẦN GÕ CHỮ BEARER)"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Database Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Bạn đang dùng Factory Pattern trong DAO, nên phải đăng ký Factory ở đây
builder.Services.AddDbContextFactory<PostgreDbContext>(options =>
    options.UseNpgsql(connectionString));

// AutoMapper
builder.Services.AddAutoMapper(typeof(DormitoryManagementSystem.Mappings.MappingProfile));

// DAOs
builder.Services.AddScoped<IUserDAO, UserDAO>();
builder.Services.AddScoped<IRoomDAO, RoomDAO>();
builder.Services.AddScoped<IBuildingDAO, BuildingDAO>();
builder.Services.AddScoped<IContractDAO, ContractDAO>();
builder.Services.AddScoped<IStudentDAO, StudentDAO>();
builder.Services.AddScoped<INewsDAO, NewsDAO>();
builder.Services.AddScoped<IPaymentDAO, PaymentDAO>();
builder.Services.AddScoped<IViolationDAO, ViolationDAO>();
builder.Services.AddScoped<IAdminDAO, AdminDAO>();
builder.Services.AddScoped<IStatisticsDAO, StatisticsDAO>();
builder.Services.AddScoped<IDashboardDAO, DashboardDAO>();


// BUSs
builder.Services.AddScoped<IUserBUS, UserBUS>();
builder.Services.AddScoped<IRoomBUS, RoomBUS>();
builder.Services.AddScoped<IBuildingBUS, BuildingBUS>();
builder.Services.AddScoped<IContractBUS, ContractBUS>();
builder.Services.AddScoped<IStudentBUS, StudentBUS>();
builder.Services.AddScoped<INewsBUS, NewsBUS>();
builder.Services.AddScoped<IPaymentBUS, PaymentBUS>();
builder.Services.AddScoped<IViolationBUS, ViolationBUS>();
builder.Services.AddScoped<IAdminBUS, AdminBUS>();
builder.Services.AddScoped<IStatisticsBUS, StatisticsBUS>();
builder.Services.AddScoped<IDashboardBUS, DashboardBUS>();

var app = builder.Build();

// Xử lí lỗi toàn cục
app.UseMiddleware<GlobalExceptionMiddleware>();
// Pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

// --- QUAN TRỌNG: BẬT XÁC THỰC TRƯỚC KHI BẬT PHÂN QUYỀN ---
app.UseAuthentication(); // <--- Dòng này cứu mạng bạn đây
app.UseAuthorization();

app.MapControllers();

app.Run();