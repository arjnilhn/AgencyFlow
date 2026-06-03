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
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public EventsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            
            var assignedEvents = await _context.Assignments
                .Include(a => a.Event)
                .Where(a => a.UserId == userId && a.Event != null)
                .Select(a => new EventListItemDto
                {
                    EventId = a.Event.Id,
                    Title = a.Event.Title,
                    DateDisplay = a.Event.StartDate.ToString("MMM dd, yyyy HH:mm"),
                    Location = a.Event.Location,
                    AssignedRole = a.AssignedRole,
                    Status = a.Event.StartDate > DateTime.Now ? "Upcoming" : "Completed"
                })
                .ToListAsync();

            var viewModel = new EventsIndexViewModel
            {
                Events = assignedEvents
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events
                .Include(e => e.Assignments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@event == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var assignment = @event.Assignments.FirstOrDefault(a => a.UserId == userId);

            var viewModel = new EventDetailsViewModel
            {
                EventId = @event.Id,
                Title = @event.Title,
                DateDisplay = @event.StartDate.ToString("MMM dd, yyyy HH:mm"),
                Location = @event.Location,
                AssignedRole = assignment?.AssignedRole,
                Status = @event.StartDate > DateTime.Now ? "Upcoming" : "Completed",
                Description = @event.Description,
                EventFee = @event.EventFee,
                Currency = @event.Currency,
                IsConfirmed = assignment?.IsConfirmed ?? false
            };

            return View(viewModel);
        }

        public IActionResult Create()
        {
            var viewModel = new CreateEventViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEventViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                
                // Create the event
                var newEvent = new Event
                {
                    Title = model.Title,
                    Description = model.Description,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Location = model.Location,
                    EventFee = model.EventFee,
                    Currency = model.Currency,
                    Status = model.Status // Use user selected status
                };

                _context.Events.Add(newEvent);
                await _context.SaveChangesAsync();

                // Automatically assign the user to this event
                var assignment = new Assignment
                {
                    EventId = newEvent.Id,
                    UserId = userId,
                    AssignedRole = model.AssignedRole ?? "Staff",
                    IsConfirmed = true
                };

                _context.Assignments.Add(assignment);
                await _context.SaveChangesAsync();

                // If event is marked as Completed, auto-generate WorkLog and Earning
                if (model.Status == "Completed" && model.EventFee > 0)
                {
                    int totalDays = (model.EndDate.Date - model.StartDate.Date).Days + 1;
                    TimeSpan dailyTime = model.EndDate.TimeOfDay - model.StartDate.TimeOfDay;
                    if (dailyTime.TotalHours < 0) dailyTime = dailyTime.Add(TimeSpan.FromHours(24));
                    decimal dailyHours = (decimal)dailyTime.TotalHours;
                    
                    decimal totalHours = dailyHours * totalDays;
                    decimal totalAmount = model.EventFee * totalDays;

                    var workLog = new WorkLog
                    {
                        AssignmentId = assignment.Id,
                        ClockIn = model.StartDate,
                        ClockOut = model.EndDate,
                        ApprovedHours = totalHours,
                        Status = "Approved"
                    };
                    _context.WorkLogs.Add(workLog);
                    await _context.SaveChangesAsync();

                    var earning = new Earning
                    {
                        WorkLogId = workLog.Id,
                        DateCalculated = model.EndDate,
                        AppliedHourlyRate = 0,
                        TotalAmount = totalAmount,
                        Currency = model.Currency,
                        PaymentStatus = "Pending"
                    };
                    _context.Earnings.Add(earning);
                    await _context.SaveChangesAsync();
                }
                TempData["SuccessMessage"] = "New event created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return NotFound();

            var viewModel = new EditEventViewModel
            {
                Id = @event.Id,
                Title = @event.Title,
                Description = @event.Description,
                StartDate = @event.StartDate,
                EndDate = @event.EndDate,
                Location = @event.Location,
                EventFee = @event.EventFee,
                Currency = @event.Currency,
                Status = @event.Status
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditEventViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var @event = await _context.Events.FindAsync(id);
                    if (@event == null) return NotFound();

                    bool statusChangedToCompleted = @event.Status != "Completed" && model.Status == "Completed";

                    @event.Title = model.Title;
                    @event.Description = model.Description;
                    @event.StartDate = model.StartDate;
                    @event.EndDate = model.EndDate;
                    @event.Location = model.Location;
                    @event.EventFee = model.EventFee;
                    @event.Currency = model.Currency;
                    @event.Status = model.Status;

                    _context.Update(@event);
                    await _context.SaveChangesAsync();

                    // Process WorkLogs/Earnings if Status is Completed
                    if (model.Status == "Completed" && model.EventFee > 0)
                    {
                        var assignments = await _context.Assignments.Where(a => a.EventId == id).ToListAsync();
                        foreach (var assignment in assignments)
                        {
                            var existingWorkLog = await _context.WorkLogs
                                .Include(w => w.Earning)
                                .FirstOrDefaultAsync(w => w.AssignmentId == assignment.Id);
                                
                            int totalDays = (model.EndDate.Date - model.StartDate.Date).Days + 1;
                            TimeSpan dailyTime = model.EndDate.TimeOfDay - model.StartDate.TimeOfDay;
                            if (dailyTime.TotalHours < 0) dailyTime = dailyTime.Add(TimeSpan.FromHours(24));
                            decimal dailyHours = (decimal)dailyTime.TotalHours;
                            
                            decimal totalHours = dailyHours * totalDays;
                            decimal totalAmount = model.EventFee * totalDays;

                            if (existingWorkLog == null)
                            {
                                // Create new
                                var workLog = new WorkLog
                                {
                                    AssignmentId = assignment.Id,
                                    ClockIn = model.StartDate,
                                    ClockOut = model.EndDate,
                                    ApprovedHours = totalHours,
                                    Status = "Approved"
                                };
                                _context.WorkLogs.Add(workLog);
                                await _context.SaveChangesAsync();

                                var earning = new Earning
                                {
                                    WorkLogId = workLog.Id,
                                    DateCalculated = model.EndDate,
                                    AppliedHourlyRate = 0,
                                    TotalAmount = totalAmount,
                                    Currency = model.Currency,
                                    PaymentStatus = "Pending"
                                };
                                _context.Earnings.Add(earning);
                                await _context.SaveChangesAsync();
                            }
                            else
                            {
                                // Update existing
                                existingWorkLog.ClockIn = model.StartDate;
                                existingWorkLog.ClockOut = model.EndDate;
                                existingWorkLog.ApprovedHours = totalHours;
                                _context.Update(existingWorkLog);

                                if (existingWorkLog.Earning != null)
                                {
                                    existingWorkLog.Earning.DateCalculated = model.EndDate;
                                    existingWorkLog.Earning.TotalAmount = totalAmount;
                                    existingWorkLog.Earning.Currency = model.Currency;
                                    _context.Update(existingWorkLog.Earning);
                                }
                                else
                                {
                                    var earning = new Earning
                                    {
                                        WorkLogId = existingWorkLog.Id,
                                        DateCalculated = model.EndDate,
                                        AppliedHourlyRate = 0,
                                        TotalAmount = totalAmount,
                                        Currency = model.Currency,
                                        PaymentStatus = "Pending"
                                    };
                                    _context.Earnings.Add(earning);
                                }
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                    else if (model.Status != "Completed")
                    {
                        // If changed back to Upcoming, remove existing earnings and worklogs to keep charts accurate
                        var assignments = await _context.Assignments
                            .Include(a => a.WorkLogs)
                                .ThenInclude(w => w.Earning)
                            .Where(a => a.EventId == id).ToListAsync();
                            
                        foreach (var assignment in assignments)
                        {
                            foreach (var workLog in assignment.WorkLogs)
                            {
                                if (workLog.Earning != null)
                                    _context.Earnings.Remove(workLog.Earning);
                                _context.WorkLogs.Remove(workLog);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(model.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events
                .Include(e => e.Assignments)
                    .ThenInclude(a => a.WorkLogs)
                        .ThenInclude(w => w.Earning)
                .FirstOrDefaultAsync(e => e.Id == id);
                
            if (@event != null)
            {
                // Delete related WorkLogs and Earnings manually if Cascade Delete is not set
                foreach (var assignment in @event.Assignments)
                {
                    foreach (var workLog in assignment.WorkLogs)
                    {
                        if (workLog.Earning != null)
                        {
                            _context.Earnings.Remove(workLog.Earning);
                        }
                        _context.WorkLogs.Remove(workLog);
                    }
                    _context.Assignments.Remove(assignment);
                }
                _context.Events.Remove(@event);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetCalendarData()
        {
            var userId = _userManager.GetUserId(User);
            var assignments = await _context.Assignments
                .Include(a => a.Event)
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var eventsForCalendar = assignments.Select(a => new
            {
                id = a.EventId,
                title = a.Event.Title,
                start = a.Event.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = a.Event.EndDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                color = a.Event.Status == "Completed" ? "#198754" : (a.Event.Status == "Ongoing" ? "#ffc107" : "#0dcaf0"),
                url = Url.Action("Details", "Events", new { id = a.EventId })
            });

            return Json(eventsForCalendar);
        }

        [AllowAnonymous]
        public async Task<IActionResult> FixCalculations()
        {
            var workLogs = await _context.WorkLogs
                .Include(w => w.Assignment)
                    .ThenInclude(a => a.Event)
                .Include(w => w.Earning)
                .ToListAsync();

            int updated = 0;
            foreach(var w in workLogs)
            {
                if (w.Assignment?.Event != null)
                {
                    var e = w.Assignment.Event;
                    int totalDays = (e.EndDate.Date - e.StartDate.Date).Days + 1;
                    TimeSpan dailyTime = e.EndDate.TimeOfDay - e.StartDate.TimeOfDay;
                    if (dailyTime.TotalHours < 0) dailyTime = dailyTime.Add(TimeSpan.FromHours(24));
                    decimal dailyHours = (decimal)dailyTime.TotalHours;
                    
                    decimal newTotalHours = dailyHours * totalDays;
                    decimal newTotalAmount = e.EventFee * totalDays;

                    w.ApprovedHours = newTotalHours;
                    _context.Update(w);

                    if (w.Earning != null)
                    {
                        w.Earning.TotalAmount = newTotalAmount;
                        _context.Update(w.Earning);
                    }
                    updated++;
                }
            }
            await _context.SaveChangesAsync();
            return Content($"Successfully updated {updated} worklogs and their earnings to use daily multiplier logic.");
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}
