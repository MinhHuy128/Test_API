namespace DormitoryManagementSystem.Utils
{
    public static class AppConstants
    {
        public static class Role
        {
            public const string Admin = "Admin";
            public const string Student = "Student";
        }
        public static class RoomStatus
        {
            public const string Active = "Active";
            public const string Inactive = "Inactive";
            public const string Maintenance = "Maintenance";
        }
        public static class ContractStatus
        {
            public const string Active = "Active";
            public const string Pending = "Pending";
            public const string Terminated = "Terminated";
            public const string Expired = "Expired";
        }
        public static class PaymentStatus
        {
            public const string Paid = "Paid";
            public const string Unpaid = "Unpaid";
            public const string Late = "Late";
        }

        public static class ViolationStatus
        {
            public const string Pending = "Pending";
            public const string Resolved = "Resolved";
            public const string Paid = "Paid";
            public const string Closed = "Closed";
        }
    }
}