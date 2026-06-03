using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgencyFlow.Models
{
    public class UserProfile
    {
        [Key]
        public required string UserId { get; set; } // PK & FK to User
        
        [Required, MaxLength(50)]
        public required string FirstName { get; set; }
        
        [Required, MaxLength(50)]
        public required string LastName { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }
        
        [MaxLength(255)]
        public string? ProfilePicture { get; set; }

        [MaxLength(50)]
        public string? City { get; set; }

        public DateTime? DateOfBirth { get; set; }

        // Navigation Property
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
