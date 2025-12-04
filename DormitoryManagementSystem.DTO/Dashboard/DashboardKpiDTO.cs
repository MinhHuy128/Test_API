namespace DormitoryManagementSystem.DTO.Dashboard
{
    public class DashboardKpiDTO
    {
        public int RoomsTotal { get; set; }
        public int RoomsAvailable { get; set; }
        public int RoomsOccupied { get; set; }
        public int ContractsPending { get; set; }
        public decimal PaymentsThisMonth { get; set; }
        public int ViolationsOpen { get; set; }
    }
}