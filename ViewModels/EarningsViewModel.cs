using System;
using System.Collections.Generic;

namespace AgencyFlow.ViewModels
{
    public class EarningsViewModel
    {
        public EarningsSummaryDto Summary { get; set; } = new EarningsSummaryDto();
        public List<EarningLogDto> EarningLogs { get; set; } = new List<EarningLogDto>();
        public ChartDataDto MonthlyEarningsChart { get; set; } = new ChartDataDto();
        public int SelectedYear { get; set; }
        public List<int> AvailableYears { get; set; } = new List<int>();
    }

    public class EarningsSummaryDto
    {
        public Dictionary<string, decimal> TotalEarningsAllTimeByCurrency { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> EarningsThisMonthByCurrency { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> PendingPaymentsByCurrency { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> PaidPaymentsByCurrency { get; set; } = new Dictionary<string, decimal>();
    }

    public class EarningLogDto
    {
        public DateTime DateCalculated { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public decimal HoursWorked { get; set; }
        public decimal AppliedHourlyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "$";
        public string PaymentStatus { get; set; } = string.Empty;
    }
}
