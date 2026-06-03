using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgencyFlow.Models
{
    public class Assignment
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int EventId { get; set; } // FK
        
        [Required]
        public required string UserId { get; set; } // FK

        [MaxLength(50)]
        public string? AssignedRole { get; set; }
        
        public bool IsConfirmed { get; set; }

        // Navigation Properties
        [ForeignKey("EventId")]
        public virtual Event? Event { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        
        public virtual ICollection<WorkLog> WorkLogs { get; set; } = new List<WorkLog>();
    }
}
