using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgencyFlow.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }
        
        [Required, MaxLength(100)]
        public required string Title { get; set; }
        
        public string? Description { get; set; }
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        [Required, MaxLength(200)]
        public required string Location { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal EventFee { get; set; } // Ücret
        
        [MaxLength(10)]
        public string Currency { get; set; } = "$";
        
        [Required, MaxLength(20)]
        public string Status { get; set; } = "Upcoming"; // Upcoming, Ongoing, Completed

        // Navigation Property
        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}
