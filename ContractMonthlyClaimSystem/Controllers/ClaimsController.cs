using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem; // Add this using
using ContractMonthlyClaimSystem.Models;
using Newtonsoft.Json;

namespace ContractMonthlyClaimSystem.Controllers // Remove extra namespace
{
    public class ClaimsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ClaimsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Claims/Create - For lecturers to submit new claims
        public async Task<IActionResult> Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userType = HttpContext.Session.GetString("UserType");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (userType != "Lecturer")
            {
                TempData["ErrorMessage"] = "Access denied. Only lecturers can submit claims.";
                return RedirectToAction("Index", "Dashboard");
            }

            var model = new CreateClaimViewModel
            {
                ClaimDate = DateTime.Today
            };

            // Get existing modules for suggestions
            ViewBag.ExistingModules = await _context.Modules
                .Where(m => m.IsActive)
                .Select(m => new { m.ModuleCode, m.HourlyRate })
                .ToListAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateClaimViewModel model, IFormFile supportingDocument)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userType = HttpContext.Session.GetString("UserType");

            if (userId == null || userType != "Lecturer")
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle manual module entry
                    var module = await HandleManualModule(model.ModuleCode, model.HourlyRate);

                    // Create claim
                    var claim = new Claim
                    {
                        LecturerId = userId.Value,
                        ModuleId = module.ModuleId,
                        ClaimDate = model.ClaimDate,
                        HoursWorked = model.HoursWorked,
                        TotalAmount = model.HoursWorked * model.HourlyRate,
                        Description = model.Description ?? string.Empty, // Handle null
                        Status = "Pending",
                        SubmittedDate = DateTime.Now
                    };

                    _context.Claims.Add(claim);
                    await _context.SaveChangesAsync();

                    // Handle file upload if provided
                    if (supportingDocument != null && supportingDocument.Length > 0)
                    {
                        await UploadSupportingDocument(claim.ClaimId, supportingDocument);
                    }

                    // Log the action
                    await LogAuditAction("Created", "Claim", claim.ClaimId, $"New claim submitted for module {model.ModuleCode}");

                    TempData["SuccessMessage"] = "Claim submitted successfully! It is now pending approval.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred while submitting the claim: {ex.Message}");
                }
            }

            // If we got this far, something failed; redisplay form
            return View(model);
        }

        // GET: Claims/Index - For lecturers to view their claims
        public async Task<IActionResult> Index(string statusFilter = "All", string sortOrder = "newest")
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userType = HttpContext.Session.GetString("UserType");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (userType != "Lecturer")
            {
                TempData["ErrorMessage"] = "Access denied. This page is for lecturers only.";
                return RedirectToAction("Index", "Dashboard");
            }

            var claimsQuery = _context.Claims
                .Include(c => c.Module)
                .Include(c => c.Approver)
                .Include(c => c.SupportingDocuments)
                .Where(c => c.LecturerId == userId);

            // Apply status filter
            if (statusFilter != "All")
            {
                claimsQuery = claimsQuery.Where(c => c.Status == statusFilter);
            }

            // Apply sorting
            claimsQuery = sortOrder switch
            {
                "oldest" => claimsQuery.OrderBy(c => c.SubmittedDate),
                "amount_high" => claimsQuery.OrderByDescending(c => c.TotalAmount),
                "amount_low" => claimsQuery.OrderBy(c => c.TotalAmount),
                _ => claimsQuery.OrderByDescending(c => c.SubmittedDate) // newest
            };

            var claims = await claimsQuery.ToListAsync();

            ViewBag.StatusFilter = statusFilter;
            ViewBag.SortOrder = sortOrder;
            ViewBag.TotalAmount = claims.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount);

            return View(claims);
        }

        // GET: Claims/Pending - For coordinators and managers to review claims
        public async Task<IActionResult> Pending(string sortOrder = "oldest")
        {
            var userType = HttpContext.Session.GetString("UserType");

            if (userType != "Coordinator" && userType != "Manager")
            {
                TempData["ErrorMessage"] = "Access denied. Only coordinators and managers can review claims.";
                return RedirectToAction("Index", "Dashboard");
            }

            var claimsQuery = _context.Claims
                .Include(c => c.Module)
                .Include(c => c.Lecturer)
                .Include(c => c.SupportingDocuments)
                .Where(c => c.Status == "Pending");

            // Apply sorting
            claimsQuery = sortOrder switch
            {
                "newest" => claimsQuery.OrderByDescending(c => c.SubmittedDate),
                "amount_high" => claimsQuery.OrderByDescending(c => c.TotalAmount),
                "amount_low" => claimsQuery.OrderBy(c => c.TotalAmount),
                _ => claimsQuery.OrderBy(c => c.SubmittedDate) // oldest
            };

            var claims = await claimsQuery.ToListAsync();
            ViewBag.SortOrder = sortOrder;

            return View(claims);
        }

        // GET: Claims/Details/5 - View claim details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var claim = await _context.Claims
                .Include(c => c.Module)
                .Include(c => c.Lecturer)
                .Include(c => c.Approver)
                .Include(c => c.SupportingDocuments)
                .FirstOrDefaultAsync(c => c.ClaimId == id);

            if (claim == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            var userType = HttpContext.Session.GetString("UserType");

            // Check if user has permission to view this claim
            if (userType != "Coordinator" && userType != "Manager" && claim.LecturerId != userId)
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Index", "Dashboard");
            }

            return View(claim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string notes = "")
        {
            return await UpdateClaimStatus(id, "Approved", "Claim approved successfully!", notes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string notes = "")
        {
            return await UpdateClaimStatus(id, "Rejected", "Claim rejected successfully!", notes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestRevision(int id, string revisionNotes)
        {
            return await UpdateClaimStatus(id, "Revision Requested", "Revision requested successfully!", revisionNotes);
        }

        private async Task<IActionResult> UpdateClaimStatus(int claimId, string status, string successMessage, string notes = "")
        {
            var userType = HttpContext.Session.GetString("UserType");

            if (userType != "Coordinator" && userType != "Manager")
            {
                TempData["ErrorMessage"] = "Access denied.";
                return RedirectToAction("Index", "Dashboard");
            }

            var claim = await _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.Module)
                .FirstOrDefaultAsync(c => c.ClaimId == claimId);

            if (claim == null)
            {
                TempData["ErrorMessage"] = "Claim not found.";
                return RedirectToAction("Pending");
            }

            var oldStatus = claim.Status;
            claim.Status = status;
            claim.ApprovedBy = HttpContext.Session.GetInt32("UserId");
            claim.ApprovedDate = DateTime.Now;

            // Add notes to description if provided
            if (!string.IsNullOrEmpty(notes))
            {
                claim.Description += $"\n\n--- {status.ToUpper()} NOTES ---\n{notes}";
            }

            try
            {
                await _context.SaveChangesAsync();

                // Log the action
                await LogAuditAction("Updated", "Claim", claim.ClaimId,
                    $"Claim status changed from {oldStatus} to {status}. Notes: {notes}");

                TempData["SuccessMessage"] = successMessage;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating the claim status: {ex.Message}";
            }

            return RedirectToAction("Pending");
        }

        // GET: Claims/Edit/5 - For lecturers to edit pending claims
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var claim = await _context.Claims
                .Include(c => c.Module)
                .FirstOrDefaultAsync(c => c.ClaimId == id);

            if (claim == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (claim.LecturerId != userId || claim.Status != "Pending")
            {
                TempData["ErrorMessage"] = "You can only edit your own pending claims.";
                return RedirectToAction("Index");
            }

            var model = new CreateClaimViewModel
            {
                ModuleCode = claim.Module.ModuleCode,
                HourlyRate = claim.Module.HourlyRate,
                ClaimDate = claim.ClaimDate,
                HoursWorked = claim.HoursWorked,
                Description = claim.Description ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateClaimViewModel model, IFormFile supportingDocument)
        {
            var claim = await _context.Claims
                .Include(c => c.Module)
                .FirstOrDefaultAsync(c => c.ClaimId == id);

            if (claim == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (claim.LecturerId != userId || claim.Status != "Pending")
            {
                TempData["ErrorMessage"] = "You can only edit your own pending claims.";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update module if changed
                    if (claim.Module.ModuleCode != model.ModuleCode || claim.Module.HourlyRate != model.HourlyRate)
                    {
                        var module = await HandleManualModule(model.ModuleCode, model.HourlyRate);
                        claim.ModuleId = module.ModuleId;
                    }

                    // Update claim details
                    claim.ClaimDate = model.ClaimDate;
                    claim.HoursWorked = model.HoursWorked;
                    claim.TotalAmount = model.HoursWorked * model.HourlyRate;
                    claim.Description = model.Description ?? string.Empty;
                    claim.SubmittedDate = DateTime.Now; // Update submission date

                    // Handle file upload if provided
                    if (supportingDocument != null && supportingDocument.Length > 0)
                    {
                        await UploadSupportingDocument(claim.ClaimId, supportingDocument);
                    }

                    await _context.SaveChangesAsync();

                    // Log the action
                    await LogAuditAction("Updated", "Claim", claim.ClaimId, "Claim updated by lecturer");

                    TempData["SuccessMessage"] = "Claim updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred while updating the claim: {ex.Message}");
                }
            }

            return View(model);
        }

        // POST: Claims/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var claim = await _context.Claims
                .Include(c => c.SupportingDocuments)
                .FirstOrDefaultAsync(c => c.ClaimId == id);

            if (claim == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (claim.LecturerId != userId || claim.Status != "Pending")
            {
                TempData["ErrorMessage"] = "You can only delete your own pending claims.";
                return RedirectToAction("Index");
            }

            try
            {
                // Delete supporting documents first
                if (claim.SupportingDocuments.Any())
                {
                    foreach (var doc in claim.SupportingDocuments)
                    {
                        var filePath = Path.Combine(_environment.WebRootPath, doc.FilePath.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                    _context.SupportingDocuments.RemoveRange(claim.SupportingDocuments);
                }

                _context.Claims.Remove(claim);
                await _context.SaveChangesAsync();

                // Log the action
                await LogAuditAction("Deleted", "Claim", claim.ClaimId, "Claim deleted by lecturer");

                TempData["SuccessMessage"] = "Claim deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the claim: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper method to handle manual module entry
        private async Task<Module> HandleManualModule(string moduleCode, decimal hourlyRate)
        {
            // Check if module already exists
            var existingModule = await _context.Modules
                .FirstOrDefaultAsync(m => m.ModuleCode.ToUpper() == moduleCode.ToUpper());

            if (existingModule != null)
            {
                return existingModule;
            }

            // Create new module
            var newModule = new Module
            {
                ModuleCode = moduleCode.ToUpper(),
                ModuleName = $"{moduleCode} - Custom Module",
                HourlyRate = hourlyRate,
                IsActive = true
            };

            _context.Modules.Add(newModule);
            await _context.SaveChangesAsync();

            return newModule;
        }

        private async Task UploadSupportingDocument(int claimId, IFormFile file)
        {
            // Validate file size (max 10MB)
            if (file.Length > 10 * 1024 * 1024)
            {
                throw new Exception("File size exceeds 10MB limit");
            }

            // Validate file type
            var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".jpg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new Exception("Only PDF, DOCX, XLSX, JPG, and PNG files are allowed");
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "documents");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var document = new SupportingDocument
            {
                ClaimId = claimId,
                FileName = file.FileName,
                FilePath = $"/uploads/documents/{uniqueFileName}",
                FileSize = file.Length,
                UploadDate = DateTime.Now
            };

            _context.SupportingDocuments.Add(document);
            await _context.SaveChangesAsync();
        }

        public async Task<IActionResult> DownloadDocument(int id)
        {
            var document = await _context.SupportingDocuments.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            var filePath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, GetContentType(filePath), document.FileName);
        }

        private string GetContentType(string path)
        {
            var types = new Dictionary<string, string>
            {
                { ".pdf", "application/pdf" },
                { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { ".jpg", "image/jpeg" },
                { ".png", "image/png" }
            };

            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }

        private async Task LogAuditAction(string action, string tableName, int recordId, string description)
        {
            var auditLog = new AuditLog
            {
                Action = action ?? "Unknown",
                TableName = tableName ?? "Unknown",
                RecordId = recordId,
                ChangedBy = HttpContext.Session.GetInt32("UserId") ?? 1, // Default to admin
                ChangedDate = DateTime.Now,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                NewValues = description ?? string.Empty
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}