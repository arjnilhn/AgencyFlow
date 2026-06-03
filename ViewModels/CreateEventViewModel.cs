using System;
using System.ComponentModel.DataAnnotations;

namespace AgencyFlow.ViewModels
{
    public class CreateEventViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Start Date is required")]
        [Display(Name = "Start Date & Time")]
        public DateTime StartDate { get; set; } = DateTime.Now.AddDays(1);

        [Required(ErrorMessage = "End Date is required")]
        [Display(Name = "End Date & Time")]
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(1).AddHours(4);

        [Required(ErrorMessage = "Location is required")]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "Event Fee")]
        public decimal EventFee { get; set; }

        public string Currency { get; set; } = "$";

        [Display(Name = "Your Role")]
        [StringLength(50)]
        public string? AssignedRole { get; set; }

        [Required]
        [Display(Name = "Event Status")]
        public string Status { get; set; } = "Upcoming";
    }
}
