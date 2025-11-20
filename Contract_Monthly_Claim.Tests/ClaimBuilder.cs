// TestDataBuilders/ClaimBuilder.cs
using ContractMonthlyClaimSystem.Models;

namespace ContractMonthlyClaimSystem
{
    public class ClaimBuilder
    {
        private Claim _claim = new Claim();

        public ClaimBuilder WithDefaultValues()
        {
            _claim = new Claim
            {
                ClaimId = 1,
                LecturerId = 1,
                ModuleId = 1,
                ClaimDate = DateTime.Now.Date,
                HoursWorked = 10,
                TotalAmount = 2500.00m,
                Description = "Test claim description",
                Status = "Pending",
                SubmittedDate = DateTime.Now,
                Lecturer = new User { UserId = 1, FullName = "John Smith" },
                Module = new Module { ModuleId = 1, ModuleCode = "PROG6212", ModuleName = "Programming 2B" }
            };
            return this;
        }

        public ClaimBuilder WithId(int id)
        {
            _claim.ClaimId = id;
            return this;
        }

        public ClaimBuilder WithStatus(string status)
        {
            _claim.Status = status;
            return this;
        }

        public ClaimBuilder WithLecturer(int lecturerId, string fullName)
        {
            _claim.LecturerId = lecturerId;
            _claim.Lecturer = new User { UserId = lecturerId, FullName = fullName };
            return this;
        }

        public ClaimBuilder WithModule(int moduleId, string moduleCode)
        {
            _claim.ModuleId = moduleId;
            _claim.Module = new Module { ModuleId = moduleId, ModuleCode = moduleCode };
            return this;
        }

        public Claim Build()
        {
            return _claim;
        }
    }
}