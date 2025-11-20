using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ContractMonthlyClaimSystem.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string UserType { get; set; } // Lecturer, Coordinator, Manager

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<Claim> Claims { get; set; }
        public ICollection<Claim> ApprovedClaims { get; set; }
        public ICollection<AuditLog> AuditLogs { get; set; }
    }
}