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
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<User> userManager)
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
            var viewModel = new DashboardViewModel { SelectedYear = targetYear };

            // Upcoming events
            var assignments = await _context.Assignments
                .Include(a => a.Event)
                .Where(a => a.UserId == userId && a.Event != null && a.Event.StartDate >= now)
                .OrderBy(a => a.Event.StartDate)
                .Take(5)
                .ToListAsync();

            viewModel.UpcomingEvents = assignments.Select(a => new DashboardEventDto
            {
                EventId = a.EventId,
                Title = a.Event.Title,
                StartDate = a.Event.StartDate,
                Location = a.Event.Location,
                AssignedRole = a.AssignedRole,
                Status = "Upcoming"
            }).ToList();

            // Earnings summary
            var userEarnings = await _context.Earnings
                .Include(e => e.WorkLog)
                .ThenInclude(w => w.Assignment)
                .Where(e => e.WorkLog != null && e.WorkLog.Assignment != null && e.WorkLog.Assignment.UserId == userId)
                .ToListAsync();

            viewModel.Summary.TotalEarningsByCurrency = userEarnings
                .GroupBy(e => string.IsNullOrWhiteSpace(e.Currency) ? "$" : e.Currency)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.TotalAmount));

            viewModel.Summary.PendingEarningsByCurrency = userEarnings
                .Where(e => e.PaymentStatus != "Paid")
                .GroupBy(e => string.IsNullOrWhiteSpace(e.Currency) ? "$" : e.Currency)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.TotalAmount));
                
            viewModel.Summary.UpcomingEventsCount = assignments.Count;
            
            var hoursThisMonth = await _context.WorkLogs
                .Include(w => w.Assignment)
                .Where(w => w.Assignment != null && w.Assignment.UserId == userId && w.ClockIn.Month == now.Month && w.ClockIn.Year == now.Year)
                .SumAsync(w => w.ApprovedHours ?? 0);
            
            viewModel.Summary.HoursWorkedThisMonth = hoursThisMonth;

            // Available Years
            var firstEarningYear = userEarnings.OrderBy(e => e.DateCalculated).Select(e => e.DateCalculated.Year).FirstOrDefault();
            int startYear = firstEarningYear > 0 ? firstEarningYear : now.Year;
            for (int y = now.Year; y >= Math.Min(startYear, now.Year - 5); y--)
            {
                if (!viewModel.AvailableYears.Contains(y)) viewModel.AvailableYears.Add(y);
            }
            if (!viewModel.AvailableYears.Contains(targetYear)) viewModel.AvailableYears.Add(targetYear);
            viewModel.AvailableYears = viewModel.AvailableYears.OrderByDescending(y => y).ToList();

            // Chart Data Generation (Full Year for Target Year)
            var earningsForYear = userEarnings.Where(e => e.DateCalculated.Year == targetYear).ToList();
            var currencies = userEarnings.Select(e => string.IsNullOrWhiteSpace(e.Currency) ? "$" : e.Currency).Distinct().ToList();
            if (!currencies.Any()) currencies.Add("$");

            for (int i = 1; i <= 12; i++)
            {
                viewModel.EarningsChart.Labels.Add(new DateTime(targetYear, i, 1).ToString("MMM"));
            }

            var colors = new string[] { "rgba(78, 115, 223, 1)", "rgba(28, 200, 138, 1)", "rgba(231, 74, 59, 1)", "rgba(246, 194, 62, 1)" };
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

                viewModel.EarningsChart.Datasets.Add(dataset);
                colorIndex++;
            }

            return View(viewModel);
        }
    }
}
