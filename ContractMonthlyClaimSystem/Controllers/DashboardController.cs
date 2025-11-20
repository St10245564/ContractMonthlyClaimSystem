using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem;
using ContractMonthlyClaimSystem.Models;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userType = HttpContext.Session.GetString("UserType");
            var userName = HttpContext.Session.GetString("FullName");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var dashboardModel = new DashboardViewModel
            {
                UserName = userName,
                UserType = userType,
                CurrentDate = DateTime.Now
            };

            switch (userType)
            {
                case "Lecturer":
                    dashboardModel = await GetLecturerDashboard((int)userId, dashboardModel);
                    break;
                case "Coordinator":
                case "Manager":
                    dashboardModel = await GetAdminDashboard(dashboardModel);
                    break;
            }

            return View(dashboardModel);
        }

        private async Task<DashboardViewModel> GetLecturerDashboard(int userId, DashboardViewModel model)
        {
            // Get lecturer's claims statistics
            var claims = await _context.Claims
                .Where(c => c.LecturerId == userId)
                .ToListAsync();

            model.TotalClaims = claims.Count;
            model.PendingClaims = claims.Count(c => c.Status == "Pending");
            model.ApprovedClaims = claims.Count(c => c.Status == "Approved");
            model.RejectedClaims = claims.Count(c => c.Status == "Rejected");
            model.TotalAmount = claims.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount);

            // Recent claims
            model.RecentClaims = await _context.Claims
                .Include(c => c.Module)
                .Where(c => c.LecturerId == userId)
                .OrderByDescending(c => c.SubmittedDate)
                .Take(5)
                .Select(c => new ClaimViewModel
                {
                    ClaimId = c.ClaimId,
                    ModuleCode = c.Module.ModuleCode,
                    ClaimDate = c.ClaimDate,
                    HoursWorked = c.HoursWorked,
                    TotalAmount = c.TotalAmount,
                    Status = c.Status,
                    SubmittedDate = c.SubmittedDate
                })
                .ToListAsync();

            // Monthly statistics for chart - FIXED QUERY
            model.MonthlyStatistics = await GetLecturerMonthlyStatistics(userId);

            return model;
        }

        private async Task<DashboardViewModel> GetAdminDashboard(DashboardViewModel model)
        {
            // Get all claims statistics
            var claims = await _context.Claims.ToListAsync();

            model.TotalClaims = claims.Count;
            model.PendingClaims = claims.Count(c => c.Status == "Pending");
            model.ApprovedClaims = claims.Count(c => c.Status == "Approved");
            model.RejectedClaims = claims.Count(c => c.Status == "Rejected");
            model.TotalAmount = claims.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount);
            model.TotalLecturers = await _context.Users.CountAsync(u => u.UserType == "Lecturer");

            // Recent claims for review
            model.RecentClaims = await _context.Claims
                .Include(c => c.Module)
                .Include(c => c.Lecturer)
                .Where(c => c.Status == "Pending")
                .OrderBy(c => c.SubmittedDate)
                .Take(5)
                .Select(c => new ClaimViewModel
                {
                    ClaimId = c.ClaimId,
                    ModuleCode = c.Module.ModuleCode,
                    LecturerName = c.Lecturer.FullName,
                    ClaimDate = c.ClaimDate,
                    HoursWorked = c.HoursWorked,
                    TotalAmount = c.TotalAmount,
                    Status = c.Status,
                    SubmittedDate = c.SubmittedDate
                })
                .ToListAsync();

            // Monthly statistics for chart - FIXED QUERY
            model.MonthlyStatistics = await GetAdminMonthlyStatistics();

            // Top lecturers by claims
            model.TopLecturers = await GetTopLecturers();

            return model;
        }

        private async Task<List<MonthlyStatistic>> GetLecturerMonthlyStatistics(int userId)
        {
            var sixMonthsAgo = DateTime.Now.AddMonths(-5);
            sixMonthsAgo = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);

            // First, get the raw data from database
            var rawData = await _context.Claims
                .Where(c => c.LecturerId == userId && c.ClaimDate >= sixMonthsAgo)
                .Select(c => new
                {
                    c.ClaimDate.Year,
                    c.ClaimDate.Month,
                    c.TotalAmount,
                    c.Status
                })
                .ToListAsync();

            // Then perform the grouping and calculations in memory
            var result = rawData
                .GroupBy(c => new { c.Year, c.Month })
                .Select(g => new MonthlyStatistic
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    ClaimsCount = g.Count(),
                    TotalAmount = g.Sum(c => c.TotalAmount),
                    ApprovedAmount = g.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount)
                })
                .OrderBy(s => s.Month)
                .ToList();

            return result;
        }

        private async Task<List<MonthlyStatistic>> GetAdminMonthlyStatistics()
        {
            var sixMonthsAgo = DateTime.Now.AddMonths(-5);
            sixMonthsAgo = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);

            // First, get the raw data from database
            var rawData = await _context.Claims
                .Where(c => c.ClaimDate >= sixMonthsAgo)
                .Select(c => new
                {
                    c.ClaimDate.Year,
                    c.ClaimDate.Month,
                    c.TotalAmount,
                    c.Status
                })
                .ToListAsync();

            // Then perform the grouping and calculations in memory
            var result = rawData
                .GroupBy(c => new { c.Year, c.Month })
                .Select(g => new MonthlyStatistic
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    ClaimsCount = g.Count(),
                    TotalAmount = g.Sum(c => c.TotalAmount),
                    ApprovedAmount = g.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount),
                    PendingCount = g.Count(c => c.Status == "Pending")
                })
                .OrderBy(s => s.Month)
                .ToList();

            return result;
        }

        private async Task<List<TopLecturer>> GetTopLecturers()
        {
            // First get approved claims with lecturer info
            var approvedClaims = await _context.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.Status == "Approved")
                .Select(c => new
                {
                    c.LecturerId,
                    c.Lecturer.FullName,
                    c.TotalAmount
                })
                .ToListAsync();

            // Then perform grouping and calculations in memory
            var result = approvedClaims
                .GroupBy(c => new { c.LecturerId, c.FullName })
                .Select(g => new TopLecturer
                {
                    LecturerName = g.Key.FullName,
                    TotalClaims = g.Count(),
                    TotalAmount = g.Sum(c => c.TotalAmount),
                    AverageAmount = g.Average(c => c.TotalAmount)
                })
                .OrderByDescending(t => t.TotalAmount)
                .Take(5)
                .ToList();

            return result;
        }

        // API endpoints for charts - FIXED QUERIES
        public async Task<JsonResult> GetClaimStatusData()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userType = HttpContext.Session.GetString("UserType");

            var claimsQuery = _context.Claims.AsQueryable();

            if (userType == "Lecturer")
            {
                claimsQuery = claimsQuery.Where(c => c.LecturerId == userId);
            }

            var claims = await claimsQuery.ToListAsync();

            var statusData = claims
                .GroupBy(c => c.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToList();

            return Json(statusData);
        }

        public async Task<JsonResult> GetMonthlyClaimData()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userType = HttpContext.Session.GetString("UserType");

            var sixMonthsAgo = DateTime.Now.AddMonths(-5);
            sixMonthsAgo = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);

            var claimsQuery = _context.Claims.Where(c => c.ClaimDate >= sixMonthsAgo);

            if (userType == "Lecturer")
            {
                claimsQuery = claimsQuery.Where(c => c.LecturerId == userId);
            }

            var claims = await claimsQuery
                .Select(c => new
                {
                    c.ClaimDate.Year,
                    c.ClaimDate.Month,
                    c.TotalAmount
                })
                .ToListAsync();

            var monthlyData = claims
                .GroupBy(c => new { c.Year, c.Month })
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    ClaimsCount = g.Count(),
                    TotalAmount = g.Sum(c => c.TotalAmount)
                })
                .OrderBy(m => m.Month)
                .ToList();

            return Json(monthlyData);
        }
    }
}