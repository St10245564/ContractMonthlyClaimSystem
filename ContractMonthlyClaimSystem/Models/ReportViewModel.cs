namespace ContractMonthlyClaimSystem.Models
{
    public class ReportViewModel
    {
        public DateTime StartDate { get; set; } = DateTime.Now.AddMonths(-1);
        public DateTime EndDate { get; set; } = DateTime.Now;
        public string ReportType { get; set; } = "MonthlySummary";
        public string UserTypeFilter { get; set; } = "All";
        public string StatusFilter { get; set; } = "All";
    }

    public class ReportResultViewModel
    {
        public string ReportTitle { get; set; }
        public DateTime GeneratedDate { get; set; } = DateTime.Now;
        public List<ClaimReportItem> Claims { get; set; } = new List<ClaimReportItem>();
        public ReportSummary Summary { get; set; } = new ReportSummary();
    }

    public class ClaimReportItem
    {
        public int ClaimId { get; set; }
        public string LecturerName { get; set; }
        public string ModuleCode { get; set; }
        public DateTime ClaimDate { get; set; }
        public decimal HoursWorked { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime SubmittedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
    }

    public class ReportSummary
    {
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ApprovedAmount { get; set; }
        public int TotalLecturers { get; set; }
    }
}