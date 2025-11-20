namespace ContractMonthlyClaimSystem.Models
{
    public class AuditLogViewModel
    {
        public int AuditLogId { get; set; }
        public string Action { get; set; } = "";
        public string TableName { get; set; } = "";
        public int RecordId { get; set; }
        public string OldValues { get; set; } = "";
        public string NewValues { get; set; } = "";
        public int ChangedBy { get; set; }
        public DateTime ChangedDate { get; set; }
        public string IPAddress { get; set; } = "";
        public string UserName { get; set; } = "";
    }
}