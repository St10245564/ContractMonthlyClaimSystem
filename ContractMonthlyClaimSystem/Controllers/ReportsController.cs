using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem;
using ContractMonthlyClaimSystem.Models;
using System.Text;
using System.Reflection;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ReportsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Reports
        public IActionResult Index()
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Coordinator" && userType != "Manager")
            {
                TempData["ErrorMessage"] = "Access denied. Only coordinators and managers can view reports.";
                return RedirectToAction("Index", "Dashboard");
            }

            var model = new ReportViewModel
            {
                StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                EndDate = DateTime.Now
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateReport(ReportViewModel model)
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Coordinator" && userType != "Manager")
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Index", "Dashboard");
            }

            var claimsQuery = _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.Module)
                .Include(c => c.Approver)
                .Where(c => c.ClaimDate >= model.StartDate && c.ClaimDate <= model.EndDate);

            // Apply filters
            if (model.StatusFilter != "All")
            {
                claimsQuery = claimsQuery.Where(c => c.Status == model.StatusFilter);
            }

            var claims = await claimsQuery
                .OrderByDescending(c => c.SubmittedDate)
                .ToListAsync();

            var reportResult = new ReportResultViewModel
            {
                ReportTitle = GetReportTitle(model.ReportType),
                GeneratedDate = DateTime.Now,
                Claims = claims.Select(c => new ClaimReportItem
                {
                    ClaimId = c.ClaimId,
                    LecturerName = c.Lecturer.FullName,
                    ModuleCode = c.Module.ModuleCode,
                    ClaimDate = c.ClaimDate,
                    HoursWorked = c.HoursWorked,
                    TotalAmount = c.TotalAmount,
                    Status = c.Status,
                    SubmittedDate = c.SubmittedDate,
                    ApprovedBy = c.Approver != null ? c.Approver.FullName : "N/A",
                    ApprovedDate = c.ApprovedDate
                }).ToList(),
                Summary = new ReportSummary
                {
                    TotalClaims = claims.Count,
                    PendingClaims = claims.Count(c => c.Status == "Pending"),
                    ApprovedClaims = claims.Count(c => c.Status == "Approved"),
                    RejectedClaims = claims.Count(c => c.Status == "Rejected"),
                    TotalAmount = claims.Sum(c => c.TotalAmount),
                    ApprovedAmount = claims.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount),
                    TotalLecturers = claims.Select(c => c.LecturerId).Distinct().Count()
                }
            };

            ViewBag.ReportParameters = model;
            return View("ReportResult", reportResult);
        }

        [HttpPost]
        public async Task<IActionResult> ExportToCsv(ReportViewModel model)
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Coordinator" && userType != "Manager")
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Index", "Dashboard");
            }

            var claimsQuery = _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.Module)
                .Include(c => c.Approver)
                .Where(c => c.ClaimDate >= model.StartDate && c.ClaimDate <= model.EndDate);

            if (model.StatusFilter != "All")
            {
                claimsQuery = claimsQuery.Where(c => c.Status == model.StatusFilter);
            }

            var claims = await claimsQuery
                .OrderByDescending(c => c.SubmittedDate)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Claim ID,Lecturer,Module,Claim Date,Hours Worked,Total Amount,Status,Submitted Date,Approved By,Approved Date");

            foreach (var claim in claims)
            {
                csv.AppendLine($"{claim.ClaimId},{EscapeCsvField(claim.Lecturer.FullName)},{EscapeCsvField(claim.Module.ModuleCode)},{claim.ClaimDate:yyyy-MM-dd},{claim.HoursWorked},{claim.TotalAmount},{EscapeCsvField(claim.Status)},{claim.SubmittedDate:yyyy-MM-dd HH:mm},{EscapeCsvField(claim.Approver?.FullName ?? "N/A")},{claim.ApprovedDate?.ToString("yyyy-MM-dd HH:mm") ?? "N/A"}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"ClaimsReport_{DateTime.Now:yyyyMMddHHmmss}.csv");
        }

        [HttpPost]
        public async Task<IActionResult> ExportToPdf(ReportViewModel model)
        {
            try
            {
                var userType = HttpContext.Session.GetString("UserType");
                if (userType != "Coordinator" && userType != "Manager")
                {
                    TempData["ErrorMessage"] = "Access denied.";
                    return RedirectToAction("Index", "Dashboard");
                }

                var claimsQuery = _context.Claims
                    .Include(c => c.Lecturer)
                    .Include(c => c.Module)
                    .Include(c => c.Approver)
                    .Where(c => c.ClaimDate >= model.StartDate && c.ClaimDate <= model.EndDate);

                if (model.StatusFilter != "All")
                {
                    claimsQuery = claimsQuery.Where(c => c.Status == model.StatusFilter);
                }

                var claims = await claimsQuery
                    .OrderByDescending(c => c.SubmittedDate)
                    .ToListAsync();

                // Generate HTML content for PDF
                var htmlContent = GeneratePdfHtmlContent(model, claims);

                // For now, return CSV as fallback since we don't have a PDF generator
                // In a real application, you would use a library like iTextSharp, QuestPDF, or WkHtmlToPdf here
                TempData["InfoMessage"] = "PDF export is currently unavailable. Downloading as CSV instead.";
                return await ExportToCsv(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while generating the PDF report.";
                return RedirectToAction("Index");
            }
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "\"\"";

            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }

            return field;
        }

        // GET: Reports/AuditLogs
        public async Task<IActionResult> AuditLogs(DateTime? startDate, DateTime? endDate, string actionFilter = "All")
        {
            try
            {
                var userType = HttpContext.Session.GetString("UserType");
                if (userType != "Manager") // Only managers can view audit logs
                {
                    TempData["ErrorMessage"] = "Access denied. Only managers can view audit logs.";
                    return RedirectToAction("Index", "Dashboard");
                }

                // Build query safely
                var query = _context.AuditLogs.AsQueryable();

                // Apply filters
                if (startDate.HasValue)
                    query = query.Where(al => al.ChangedDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(al => al.ChangedDate <= endDate.Value);

                if (!string.IsNullOrEmpty(actionFilter) && actionFilter != "All")
                    query = query.Where(al => al.Action == actionFilter);

                // Load data with explicit null handling
                var logs = await query
                    .Include(al => al.User)
                    .OrderByDescending(al => al.ChangedDate)
                    .Take(1000)
                    .ToListAsync();

                // Transform data with null safety
                var viewModel = logs.Select(al => new AuditLogViewModel
                {
                    AuditLogId = al.AuditLogId,
                    Action = al.Action ?? "Unknown",
                    TableName = al.TableName ?? "Unknown",
                    RecordId = al.RecordId,
                    OldValues = al.OldValues ?? "",
                    NewValues = al.NewValues ?? "",
                    ChangedBy = al.ChangedBy,
                    ChangedDate = al.ChangedDate,
                    IPAddress = al.IPAddress ?? "Unknown",
                    UserName = al.User?.FullName ?? "Unknown User"
                }).ToList();

                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                ViewBag.ActionFilter = actionFilter;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the error and show user-friendly message
                TempData["ErrorMessage"] = "An error occurred while loading audit logs. Please try again.";
                // In production, you might want to log the actual exception
                return RedirectToAction("Index");
            }
        }

        private string GetReportTitle(string reportType)
        {
            return reportType switch
            {
                "MonthlySummary" => "Monthly Claims Summary Report",
                "LecturerPerformance" => "Lecturer Performance Report",
                "FinancialSummary" => "Financial Summary Report",
                _ => "Claims Report"
            };
        }

        private string GeneratePdfHtmlContent(ReportViewModel model, List<Claim> claims)
        {
            var summary = new ReportSummary
            {
                TotalClaims = claims.Count,
                PendingClaims = claims.Count(c => c.Status == "Pending"),
                ApprovedClaims = claims.Count(c => c.Status == "Approved"),
                RejectedClaims = claims.Count(c => c.Status == "Rejected"),
                TotalAmount = claims.Sum(c => c.TotalAmount),
                ApprovedAmount = claims.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount),
                TotalLecturers = claims.Select(c => c.LecturerId).Distinct().Count()
            };

            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
            html.AppendLine("h1 { color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 10px; }");
            html.AppendLine("h2 { color: #34495e; margin-top: 20px; }");
            html.AppendLine(".summary { background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin-bottom: 20px; }");
            html.AppendLine(".summary-item { margin: 5px 0; }");
            html.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 10px; }");
            html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            html.AppendLine("th { background-color: #3498db; color: white; }");
            html.AppendLine("tr:nth-child(even) { background-color: #f2f2f2; }");
            html.AppendLine(".total-row { font-weight: bold; background-color: #e8f4f8 !important; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            // Header
            html.AppendLine($"<h1>{GetReportTitle(model.ReportType)}</h1>");
            html.AppendLine($"<p><strong>Generated:</strong> {DateTime.Now:yyyy-MM-dd HH:mm}</p>");
            html.AppendLine($"<p><strong>Period:</strong> {model.StartDate:yyyy-MM-dd} to {model.EndDate:yyyy-MM-dd}</p>");
            html.AppendLine($"<p><strong>Status Filter:</strong> {model.StatusFilter}</p>");

            // Summary
            html.AppendLine("<div class='summary'>");
            html.AppendLine("<h2>Summary</h2>");
            html.AppendLine($"<div class='summary-item'><strong>Total Claims:</strong> {summary.TotalClaims}</div>");
            html.AppendLine($"<div class='summary-item'><strong>Pending Claims:</strong> {summary.PendingClaims}</div>");
            html.AppendLine($"<div class='summary-item'><strong>Approved Claims:</strong> {summary.ApprovedClaims}</div>");
            html.AppendLine($"<div class='summary-item'><strong>Rejected Claims:</strong> {summary.RejectedClaims}</div>");
            html.AppendLine($"<div class='summary-item'><strong>Total Lecturers:</strong> {summary.TotalLecturers}</div>");
            html.AppendLine($"<div class='summary-item'><strong>Total Amount:</strong> R {summary.TotalAmount:N2}</div>");
            html.AppendLine($"<div class='summary-item'><strong>Approved Amount:</strong> R {summary.ApprovedAmount:N2}</div>");
            html.AppendLine("</div>");

            // Claims Table
            html.AppendLine("<h2>Claims Details</h2>");
            html.AppendLine("<table>");
            html.AppendLine("<thead>");
            html.AppendLine("<tr>");
            html.AppendLine("<th>Claim ID</th>");
            html.AppendLine("<th>Lecturer</th>");
            html.AppendLine("<th>Module</th>");
            html.AppendLine("<th>Claim Date</th>");
            html.AppendLine("<th>Hours</th>");
            html.AppendLine("<th>Amount</th>");
            html.AppendLine("<th>Status</th>");
            html.AppendLine("<th>Submitted</th>");
            html.AppendLine("<th>Approved By</th>");
            html.AppendLine("</tr>");
            html.AppendLine("</thead>");
            html.AppendLine("<tbody>");

            foreach (var claim in claims)
            {
                html.AppendLine("<tr>");
                html.AppendLine($"<td>{claim.ClaimId}</td>");
                html.AppendLine($"<td>{claim.Lecturer.FullName}</td>");
                html.AppendLine($"<td>{claim.Module.ModuleCode}</td>");
                html.AppendLine($"<td>{claim.ClaimDate:yyyy-MM-dd}</td>");
                html.AppendLine($"<td>{claim.HoursWorked}</td>");
                html.AppendLine($"<td>R {claim.TotalAmount:N2}</td>");
                html.AppendLine($"<td>{claim.Status}</td>");
                html.AppendLine($"<td>{claim.SubmittedDate:yyyy-MM-dd HH:mm}</td>");
                html.AppendLine($"<td>{claim.Approver?.FullName ?? "N/A"}</td>");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody>");
            html.AppendLine("</table>");

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }
    }
}