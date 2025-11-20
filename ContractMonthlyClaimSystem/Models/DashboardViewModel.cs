namespace ContractMonthlyClaimSystem.Models
{
    public class DashboardViewModel
    {
        public string UserName { get; set; }
        public string UserType { get; set; }
        public DateTime CurrentDate { get; set; }

        // Statistics
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalLecturers { get; set; }

        // Lists
        public List<ClaimViewModel> RecentClaims { get; set; } = new List<ClaimViewModel>();
        public List<MonthlyStatistic> MonthlyStatistics { get; set; } = new List<MonthlyStatistic>();
        public List<TopLecturer> TopLecturers { get; set; } = new List<TopLecturer>();
    }

    public class ClaimViewModel
    {
        public int ClaimId { get; set; }
        public string ModuleCode { get; set; }
        public string LecturerName { get; set; }
        public DateTime ClaimDate { get; set; }
        public decimal HoursWorked { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime SubmittedDate { get; set; }

        public string FormattedAmount => TotalAmount.ToString("C");
        public string FormattedDate => ClaimDate.ToString("MMM dd, yyyy");
        public string StatusBadgeClass => Status switch
        {
            "Pending" => "warning",
            "Approved" => "success",
            "Rejected" => "danger",
            _ => "secondary"
        };
    }

    public class MonthlyStatistic
    {
        public string Month { get; set; }
        public int ClaimsCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ApprovedAmount { get; set; }
        public int PendingCount { get; set; }

        public string FormattedTotalAmount => TotalAmount.ToString("C");
        public string FormattedApprovedAmount => ApprovedAmount.ToString("C");
    }

    public class TopLecturer
    {
        public string LecturerName { get; set; }
        public int TotalClaims { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }

        public string FormattedTotalAmount => TotalAmount.ToString("C");
        public string FormattedAverageAmount => AverageAmount.ToString("C");
    }
}