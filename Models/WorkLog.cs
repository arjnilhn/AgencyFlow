using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgencyFlow.Models
{
    public class WorkLog
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int AssignmentId { get; set; } // FK
        
        public DateTime ClockIn { get; set; }
        public DateTime? ClockOut { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ApprovedHours { get; set; }
        
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        // Navigation Properties
        [ForeignKey("AssignmentId")]
        public virtual Assignment? Assignment { get; set; }
        
        public virtual Earning? Earning { get; set; }
    }
}
