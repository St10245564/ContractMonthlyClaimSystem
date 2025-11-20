using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ContractMonthlyClaimSystem
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor = null)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<SupportingDocument> SupportingDocuments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();

                entity.Property(u => u.Username)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(u => u.Password)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.UserType)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(u => u.FullName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(u => u.IsActive)
                    .HasDefaultValue(true);
            });

            // Configure Module entity
            modelBuilder.Entity<Module>(entity =>
            {
                entity.HasKey(m => m.ModuleId);
                entity.HasIndex(m => m.ModuleCode).IsUnique();

                entity.Property(m => m.ModuleCode)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(m => m.ModuleName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(m => m.HourlyRate)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                entity.Property(m => m.IsActive)
                    .HasDefaultValue(true);

                entity.Property(m => m.CreatedDate)
                    .HasDefaultValueSql("GETDATE()");
            });

            // Configure Claim entity
            modelBuilder.Entity<Claim>(entity =>
            {
                entity.HasKey(c => c.ClaimId);

                entity.Property(c => c.ClaimDate)
                    .HasColumnType("date");

                entity.Property(c => c.HoursWorked)
                    .HasColumnType("decimal(5,2)")
                    .IsRequired();

                entity.Property(c => c.TotalAmount)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

                entity.Property(c => c.Description)
                    .HasMaxLength(500);

                entity.Property(c => c.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("Pending");

                entity.Property(c => c.SubmittedDate)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(c => c.ApprovedDate)
                    .IsRequired(false);

                // Relationships
                entity.HasOne(c => c.Lecturer)
                    .WithMany(u => u.Claims)
                    .HasForeignKey(c => c.LecturerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Approver)
                    .WithMany(u => u.ApprovedClaims)
                    .HasForeignKey(c => c.ApprovedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Module)
                    .WithMany(m => m.Claims)
                    .HasForeignKey(c => c.ModuleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure SupportingDocument entity
            modelBuilder.Entity<SupportingDocument>(entity =>
            {
                entity.HasKey(sd => sd.DocumentId);

                entity.Property(sd => sd.FileName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(sd => sd.FilePath)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(sd => sd.FileSize);

                entity.Property(sd => sd.UploadDate)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasOne(sd => sd.Claim)
                    .WithMany(c => c.SupportingDocuments)
                    .HasForeignKey(sd => sd.ClaimId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure AuditLog entity
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(al => al.AuditLogId);

                entity.Property(al => al.Action)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(al => al.TableName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(al => al.RecordId)
                    .IsRequired();

                entity.Property(al => al.OldValues)
                    .HasColumnType("nvarchar(max)");

                entity.Property(al => al.NewValues)
                    .HasColumnType("nvarchar(max)");

                entity.Property(al => al.ChangedBy)
                    .IsRequired();

                entity.Property(al => al.ChangedDate)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(al => al.IPAddress)
                    .HasMaxLength(45);

                entity.HasOne(al => al.User)
                    .WithMany(u => u.AuditLogs)
                    .HasForeignKey(al => al.ChangedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Modules
            modelBuilder.Entity<Module>().HasData(
                new Module { ModuleId = 1, ModuleCode = "PROG6212", ModuleName = "Programming 2B", HourlyRate = 250.00m, IsActive = true, CreatedDate = new DateTime(2024, 1, 1) },
                new Module { ModuleId = 2, ModuleCode = "DBAS6211", ModuleName = "Database Systems", HourlyRate = 280.00m, IsActive = true, CreatedDate = new DateTime(2024, 1, 1) },
                new Module { ModuleId = 3, ModuleCode = "WEBD6211", ModuleName = "Web Development", HourlyRate = 270.00m, IsActive = true, CreatedDate = new DateTime(2024, 1, 1) },
                new Module { ModuleId = 4, ModuleCode = "NETW6211", ModuleName = "Networking Fundamentals", HourlyRate = 260.00m, IsActive = true, CreatedDate = new DateTime(2024, 1, 1) },
                new Module { ModuleId = 5, ModuleCode = "MATH6211", ModuleName = "Mathematics for Computing", HourlyRate = 240.00m, IsActive = true, CreatedDate = new DateTime(2024, 1, 1) }
            );

            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User { UserId = 1, Username = "lecturer1", Password = "password123", Email = "lecturer1@email.com", UserType = "Lecturer", FullName = "Dr. John Smith", CreatedDate = new DateTime(2024, 1, 1), IsActive = true },
                new User { UserId = 2, Username = "lecturer2", Password = "password123", Email = "lecturer2@email.com", UserType = "Lecturer", FullName = "Prof. Sarah Johnson", CreatedDate = new DateTime(2024, 1, 1), IsActive = true },
                new User { UserId = 3, Username = "coordinator1", Password = "password123", Email = "coordinator1@email.com", UserType = "Coordinator", FullName = "Ms. Sarah Wilson", CreatedDate = new DateTime(2024, 1, 1), IsActive = true },
                new User { UserId = 4, Username = "manager1", Password = "password123", Email = "manager1@email.com", UserType = "Manager", FullName = "Mr. Michael Brown", CreatedDate = new DateTime(2024, 1, 1), IsActive = true }
            );

            // Seed some sample claims
            modelBuilder.Entity<Claim>().HasData(
                new Claim { ClaimId = 1, LecturerId = 1, ModuleId = 1, ClaimDate = new DateTime(2024, 1, 15), HoursWorked = 8.0m, TotalAmount = 2000.00m, Description = "Lecture delivery and student consultations", Status = "Approved", SubmittedDate = new DateTime(2024, 1, 16), ApprovedBy = 3, ApprovedDate = new DateTime(2024, 1, 18) },
                new Claim { ClaimId = 2, LecturerId = 1, ModuleId = 2, ClaimDate = new DateTime(2024, 1, 20), HoursWorked = 6.5m, TotalAmount = 1820.00m, Description = "Practical session and assessment marking", Status = "Pending", SubmittedDate = new DateTime(2024, 1, 21) },
                new Claim { ClaimId = 3, LecturerId = 2, ModuleId = 3, ClaimDate = new DateTime(2024, 1, 10), HoursWorked = 10.0m, TotalAmount = 2700.00m, Description = "Workshop facilitation and project supervision", Status = "Approved", SubmittedDate = new DateTime(2024, 1, 11), ApprovedBy = 4, ApprovedDate = new DateTime(2024, 1, 14) }
            );
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var auditEntries = OnBeforeSaveChanges();
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            await OnAfterSaveChanges(auditEntries);
            return result;
        }

        public override int SaveChanges()
        {
            var auditEntries = OnBeforeSaveChanges();
            var result = base.SaveChanges();
            OnAfterSaveChanges(auditEntries).Wait();
            return result;
        }

        private List<AuditLog> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditLog>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditLog
                {
                    TableName = entry.Entity.GetType().Name,
                    Action = entry.State.ToString(),
                    ChangedDate = DateTime.Now,
                    IPAddress = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown"
                };

                // Get the primary key value
                var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                if (primaryKey != null)
                {
                    auditEntry.RecordId = Convert.ToInt32(primaryKey.CurrentValue);
                }

                // Get user ID from session if available
                var httpContext = _httpContextAccessor?.HttpContext;
                if (httpContext != null)
                {
                    var userId = httpContext.Session.GetInt32("UserId");
                    auditEntry.ChangedBy = userId ?? 1; // Default to admin if no user in session
                }
                else
                {
                    auditEntry.ChangedBy = 1; // Default user
                }

                foreach (var property in entry.Properties)
                {
                    if (property.IsTemporary)
                        continue;

                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.NewValues += $"{propertyName}: {property.CurrentValue}; ";
                            break;

                        case EntityState.Deleted:
                            auditEntry.OldValues += $"{propertyName}: {property.OriginalValue}; ";
                            break;

                        case EntityState.Modified:
                            if (!object.Equals(property.OriginalValue, property.CurrentValue))
                            {
                                auditEntry.OldValues += $"{propertyName}: {property.OriginalValue}; ";
                                auditEntry.NewValues += $"{propertyName}: {property.CurrentValue}; ";
                            }
                            break;
                    }
                }

                auditEntries.Add(auditEntry);
            }

            return auditEntries;
        }

        private Task OnAfterSaveChanges(List<AuditLog> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return Task.CompletedTask;

            // Add all audit entries
            AuditLogs.AddRange(auditEntries);
            return SaveChangesAsync();
        }
    }
}