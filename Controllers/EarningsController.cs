using AgencyFlow.Data;
using AgencyFlow.Models;
using AgencyFlow.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AgencyFlow.Controllers
{
    [Authorize]
    public class EarningsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public EarningsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? year)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return NotFound();

            var now = DateTime.Now;
            int targetYear = year ?? now.Year;
            var viewModel = new EarningsViewModel { SelectedYear = targetYear };

            var earnings = await _context.Earnings
                .Include(e => e.WorkLog)
                .ThenInclude(w => w.Assignment)
                .ThenInclude(a => a.Event)
                .Where(e => e.WorkLog != null && e.WorkLog.Assignment != null && e.WorkLog.Assignment.UserId == userId)
                .OrderByDescending(e => e.DateCalculated)
                .ToListAsync();

            viewModel.Summary.TotalEarningsAllTimeByCurrency = earnings
                .GroupBy(e => string.IsNullOrWhiteSpace(e.Currency) ? "$" : e.Currency)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.TotalAmount));

            viewModel.Summary.EarningsThisMonthByCurrency = earnings
                .Where(e => e.DateCalculated.Month == now.Month && e.DateCalculated.Year == now.Year)
                .GroupBy(e => string.IsNullOrWhiteSpace(e.Currency) ? "$" : e.Currency)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.TotalAmount));

            viewModel.Summary.PendingPaymentsByCurrency = earnings
                .Where(e => e.PaymentStatus != "Paid")
                .GroupBy(e => string.IsNullOrWhiteSpace(e.Currency) ? "$" : e.Currency)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.TotalAmount));

            viewModel.Summary.PaidPaymentsByCurrency = earnings
                .Where(e => e.PaymentStatus == "Paid")
                .GroupBy(e => string.IsNullOrWhiteSpace(e.Currency) ? "$" : e.Currency)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.TotalAmount));

            // EarningLogs list is not filtered by year so they can see history, or we can filter it. Let's keep it all or maybe just year filter.
            // Wait, standard practice is that the whole page changes based on year. Let's filter logs by year as well.
            var earningsForYear = earnings.Where(e => e.DateCalculated.Year == targetYear).ToList();

            viewModel.EarningLogs = earningsForYear.Select(e => new EarningLogDto
            {
                DateCalculated = e.DateCalculated,
                EventTitle = e.WorkLog?.Assignment?.Event?.Title ?? "Unknown",
                HoursWorked = e.WorkLog?.ApprovedHours ?? 0,
                AppliedHourlyRate = e.AppliedHourlyRate,
                TotalAmount = e.TotalAmount,
                Currency = e.Currency ?? "$",
                PaymentStatus = e.PaymentStatus == "Paid" ? "Paid" : "Pending"
            }).ToList();

            // Available Years
            var firstEarningYear = earnings.OrderBy(e => e.DateCalculated).Select(e => e.DateCalculated.Year).FirstOrDefault();
            int startYear = firstEarningYear > 0 ? firstEarningYear : now.Year;
            for (int y = now.Year; y >= Math.Min(startYear, now.Year - 5); y--)
            {
                if (!viewModel.AvailableYears.Contains(y)) viewModel.AvailableYears.Add(y);
            }
            if (!viewModel.AvailableYears.Contains(targetYear)) viewModel.AvailableYears.Add(targetYear);
            viewModel.AvailableYears = viewModel.AvailableYears.OrderByDescending(y => y).ToList();

            // Chart Data Generation (Full Year for Target Year)
            var currencies = earnings.Select(e => string.IsNullOrWhiteSpace(e.Currency) ? "$" : e.Currency).Distinct().ToList();
            if (!currencies.Any()) currencies.Add("$");

            for (int i = 1; i <= 12; i++)
            {
                viewModel.MonthlyEarningsChart.Labels.Add(new DateTime(targetYear, i, 1).ToString("MMM"));
            }

            var colors = new string[] { "rgba(28, 200, 138, 1)", "rgba(78, 115, 223, 1)", "rgba(231, 74, 59, 1)", "rgba(246, 194, 62, 1)" };
            int colorIndex = 0;

            foreach (var currency in currencies)
            {
                var dataset = new ChartDatasetDto
                {
                    Label = currency,
                    BorderColor = colors[colorIndex % colors.Length]
                };

                var currencyEarnings = earningsForYear.Where(e => (string.IsNullOrWhiteSpace(e.Currency) ? "$" : e.Currency) == currency).ToList();

                for (int month = 1; month <= 12; month++)
                {
                    var sum = currencyEarnings.Where(e => e.DateCalculated.Month == month).Sum(e => e.TotalAmount);
                    dataset.Data.Add(sum);
                }

                viewModel.MonthlyEarningsChart.Datasets.Add(dataset);
                colorIndex++;
            }

            return View(viewModel);
        }
    }
}
