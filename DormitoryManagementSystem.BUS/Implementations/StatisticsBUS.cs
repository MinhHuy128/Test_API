using DormitoryManagementSystem.BUS.Interfaces;
using DormitoryManagementSystem.DAO.Interfaces;
using DormitoryManagementSystem.DTO.Statistics;

namespace DormitoryManagementSystem.BUS.Implementations
{
    public class StatisticsBUS : IStatisticsBUS
    {
        private readonly IStatisticsDAO _dao;
        public StatisticsBUS(IStatisticsDAO dao) => _dao = dao;

        public async Task<DashboardStatsDTO> GetDashboardStatsAsync() => await _dao.GetDashboardStatsAsync();

        public async Task<IEnumerable<RevenueStatsDTO>> GetMonthlyRevenueAsync(int year)
        {
            var dbStats = await _dao.GetMonthlyRevenueAsync(year);
            var fullStats = new List<RevenueStatsDTO>();
            for (int m = 1; m <= 12; m++)
            {
                var stat = dbStats.FirstOrDefault(s => s.Month == m);
                fullStats.Add(stat ?? new RevenueStatsDTO { Month = m, Year = year, Revenue = 0 });
            }
            return fullStats;
        }

        public async Task<IEnumerable<OccupancyStatsDTO>> GetOccupancyTrendAsync(int year)
        {
            var contracts = await _dao.GetContractsByYearAsync(year);
            var result = new List<OccupancyStatsDTO>();
            for (int m = 1; m <= 12; m++)
            {
                var start = new DateOnly(year, m, 1);
                var end = start.AddMonths(1).AddDays(-1);
                int count = contracts.Count(c => c.Starttime <= end && c.Endtime >= start);
                result.Add(new OccupancyStatsDTO { Month = m, Year = year, TotalStudents = count });
            }
            return result;
        }

        public async Task<GenderStatsDTO> GetGenderStatsAsync() => await _dao.GetGenderStatsAsync();

        public async Task<IEnumerable<BuildingComparisonDTO>> GetBuildingComparisonAsync(int? year) =>
            await _dao.GetBuildingComparisonAsync(year ?? DateTime.Now.Year);

        public async Task<IEnumerable<ViolationTrendDTO>> GetViolationTrendAsync(int year)
        {
            var dbStats = await _dao.GetViolationTrendAsync(year);
            var fullStats = new List<ViolationTrendDTO>();
            for (int m = 1; m <= 12; m++)
            {
                var stat = dbStats.FirstOrDefault(s => s.Month == m);
                fullStats.Add(stat ?? new ViolationTrendDTO { Month = m, Year = year, ViolationCount = 0 });
            }
            return fullStats;
        }

        public async Task<ViolationSummaryDTO> GetViolationSummaryStatsAsync() => await _dao.GetViolationSummaryStatsAsync();

        public async Task<PaymentStatsDTO> GetPaymentStatisticsAsync() => await _dao.GetPaymentStatisticsAsync();
    }
}