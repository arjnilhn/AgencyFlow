using System;
using System.Collections.Generic;

namespace AgencyFlow.ViewModels
{
    public class DashboardViewModel
    {
        public DashboardSummaryDto Summary { get; set; } = new DashboardSummaryDto();
        public List<DashboardEventDto> UpcomingEvents { get; set; } = new List<DashboardEventDto>();
        public ChartDataDto EarningsChart { get; set; } = new ChartDataDto();
        public int SelectedYear { get; set; }
        public List<int> AvailableYears { get; set; } = new List<int>();
    }

    public class DashboardSummaryDto
    {
        public Dictionary<string, decimal> TotalEarningsByCurrency { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> PendingEarningsByCurrency { get; set; } = new Dictionary<string, decimal>();
        public int UpcomingEventsCount { get; set; }
        public decimal HoursWorkedThisMonth { get; set; }
    }

    public class DashboardEventDto
    {
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? AssignedRole { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
