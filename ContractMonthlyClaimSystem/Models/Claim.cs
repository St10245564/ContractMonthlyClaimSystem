using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.Models
{
    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        [Required]
        public int LecturerId { get; set; }

        [Required]
        public int ModuleId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ClaimDate { get; set; }

        [Required]
        [Range(0.5, 100, ErrorMessage = "Hours worked must be between 0.5 and 100")]
        public decimal HoursWorked { get; set; }

        public decimal TotalAmount { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public DateTime SubmittedDate { get; set; } = DateTime.Now;

        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        // Navigation properties
        public User Lecturer { get; set; }
        public Module Module { get; set; }
        public User Approver { get; set; }
        public ICollection<SupportingDocument> SupportingDocuments { get; set; }
    }
}