using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgencyFlow.Models
{
    public class Earning
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int WorkLogId { get; set; } // FK (1-to-1 with WorkLog)
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(10)]
        public string Currency { get; set; } = "$";
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal AppliedHourlyRate { get; set; }
        
        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid
        
        public DateTime DateCalculated { get; set; } = DateTime.UtcNow;

        // Navigation Property
        [ForeignKey("WorkLogId")]
        public virtual WorkLog? WorkLog { get; set; }
    }
}
