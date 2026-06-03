using System;
using System.Collections.Generic;

namespace AgencyFlow.ViewModels
{
    public class EventsIndexViewModel
    {
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public List<EventListItemDto> Events { get; set; } = new List<EventListItemDto>();
    }

    public class EventListItemDto
    {
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string DateDisplay { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AssignedRole { get; set; }
    }

    public class EventDetailsViewModel : EventListItemDto
    {
        public string? Description { get; set; }
        public decimal EventFee { get; set; }
        public string Currency { get; set; } = "$";
        public bool IsConfirmed { get; set; }
    }
}
