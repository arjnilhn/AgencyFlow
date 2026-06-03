using System;
using System.Collections.Generic;

namespace AgencyFlow.ViewModels
{
    public class WorkHoursViewModel
    {
        public WorkHoursSummaryDto Summary { get; set; } = new WorkHoursSummaryDto();
        public List<WorkLogDto> WorkLogs { get; set; } = new List<WorkLogDto>();
    }

    public class WorkHoursSummaryDto
    {
        public decimal TotalApprovedHours { get; set; }
        public decimal PendingApprovalHours { get; set; }
    }

    public class WorkLogDto
    {
        public int LogId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string? AssignedRole { get; set; }
        public DateTime ClockIn { get; set; }
        public DateTime? ClockOut { get; set; }
        public decimal? ApprovedHours { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
