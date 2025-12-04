using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DormitoryManagementSystem.BUS.Interfaces;
using DormitoryManagementSystem.DTO.Dashboard;

namespace DormitoryManagementSystem.API.Controllers
{
    [Route("api/admin/dashboard")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Chỉ Admin mới xem được Dashboard
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardBUS _dashboardBUS;

        // Dependency Injection
        public DashboardController(IDashboardBUS dashboardBUS)
        {
            _dashboardBUS = dashboardBUS;
        }

        // GET: api/admin/dashboard/buildings
        [HttpGet("buildings")]
        public async Task<ActionResult<BuildingKpiResponseDTO>> GetBuildingKpis()
        {
            try
            {
                var result = await _dashboardBUS.GetBuildingKpisAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log error here (ví dụ dùng Serilog)
                return StatusCode(500, new { message = "Lỗi hệ thống khi tải danh sách tòa nhà.", detail = ex.Message });
            }
        }

        // GET: api/admin/dashboard/kpis?building=A&from=2023-01-01&to=2023-02-01
        [HttpGet("kpis")]
        public async Task<ActionResult<DashboardKpiDTO>> GetDashboardKpis(
            [FromQuery] string? building,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var result = await _dashboardBUS.GetDashboardKpisAsync(building, from, to);
            return Ok(result);
        }

        // GET: api/admin/dashboard/charts
        [HttpGet("charts")]
        public async Task<ActionResult<DashboardChartsDTO>> GetDashboardCharts(
            [FromQuery] string? building,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var result = await _dashboardBUS.GetDashboardChartsAsync(building, from, to);
            return Ok(result);
        }

        // GET: api/admin/dashboard/alerts
        [HttpGet("alerts")]
        public async Task<ActionResult<List<AlertDTO>>> GetAlerts()
        {
            var result = await _dashboardBUS.GetAlertsAsync();
            return Ok(result);
        }

        // GET: api/admin/dashboard/activities?limit=10
        [HttpGet("activities")]
        public async Task<ActionResult<List<ActivityDTO>>> GetActivities([FromQuery] int limit = 20)
        {
            var result = await _dashboardBUS.GetActivitiesAsync(limit);
            return Ok(result);
        }
    }
}