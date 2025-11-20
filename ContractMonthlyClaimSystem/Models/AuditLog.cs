using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.Models
{
    public class AuditLog
    {
        [Key]
        public int AuditLogId { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = ""; // Created, Updated, Deleted

        [Required]
        [StringLength(50)]
        public string TableName { get; set; } = "";

        [Required]
        public int RecordId { get; set; }

        public string? OldValues { get; set; } = ""; // Changed to nullable with default

        public string? NewValues { get; set; } = ""; // Changed to nullable with default

        [Required]
        public int ChangedBy { get; set; }

        public DateTime ChangedDate { get; set; } = DateTime.Now;

        [StringLength(45)]
        public string? IPAddress { get; set; } = ""; // Changed to nullable with default

        // Navigation properties
        public virtual User? User { get; set; }
    }
}