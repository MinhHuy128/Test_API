using System.Text;
using AutoMapper;
using DormitoryManagementSystem.BUS.Implementations;
using DormitoryManagementSystem.BUS.Interfaces;
using DormitoryManagementSystem.DAO.Context;
using DormitoryManagementSystem.DAO.Implementations;
using DormitoryManagementSystem.DAO.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- CẤU HÌNH JWT TRƯỚC ---
var secretKey = builder.Configuration["Jwt:Key"] ?? "DayLaCaiKeyBiMatCuaNhomChungToiKhongDuocTietLo123456";

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

        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),

        ValidateIssuer = false,
        ValidateAudience = false,

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// --- CẤU HÌNH SWAGGER CHUẨN ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dormitory API", Version = "v1" });

    // Định nghĩa Bearer Token
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http, // Dùng Http thay vì ApiKey để nó tự hiểu chuẩn Bearer
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập Token vào ô bên dưới (Chỉ dán mã Token, không cần gõ chữ Bearer)"
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
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Database Context (Factory Pattern)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// LƯU Ý: Bạn đang dùng Factory trong DAO nên ở đây phải đăng ký Factory
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
// Nhớ đăng ký DAO mới nếu có (PendingBillDAO...)

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

var app = builder.Build();

// Pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

// QUAN TRỌNG: Authentication phải trước Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();