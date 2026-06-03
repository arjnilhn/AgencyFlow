using AgencyFlow.Data;
using AgencyFlow.Models;
using AgencyFlow.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AgencyFlow.Controllers
{
    [Authorize]
    public class WorkHoursController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public WorkHoursController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var workLogs = await _context.WorkLogs
                .Include(w => w.Assignment)
                .ThenInclude(a => a.Event)
                .Where(w => w.Assignment != null && w.Assignment.UserId == userId)
                .OrderByDescending(w => w.ClockIn)
                .ToListAsync();

            var viewModel = new WorkHoursViewModel();
            viewModel.Summary.TotalApprovedHours = workLogs.Where(w => w.Status == "Approved").Sum(w => w.ApprovedHours ?? 0);
            viewModel.Summary.PendingApprovalHours = workLogs.Where(w => w.Status == "Pending").Sum(w => w.ApprovedHours ?? 0);
            
            viewModel.WorkLogs = workLogs.Select(w => new WorkLogDto
            {
                LogId = w.Id,
                EventTitle = w.Assignment?.Event?.Title ?? "Unknown",
                AssignedRole = w.Assignment?.AssignedRole,
                ClockIn = w.ClockIn,
                ClockOut = w.ClockOut,
                ApprovedHours = w.ApprovedHours,
                Status = w.Status
            }).ToList();

            return View(viewModel);
        }
    }
}
