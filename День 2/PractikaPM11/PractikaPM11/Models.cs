namespace PractikaPM11
{
    public class User
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }

    public class Department
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; }
    }

    public class Employee
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
    }

    public class VisitPurpose
    {
        public int PurposeId { get; set; }
        public string Name { get; set; }
    }

    public class Visitor
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? Phone { get; set; }
        public string Email { get; set; }
        public string? Organization { get; set; }
        public DateOnly BirthDate { get; set; }
        public string PassportSeries { get; set; }
        public string PassportNumber { get; set; }
        public string PassportScanPath { get; set; }
    }

    public class RequestInfo
    {
        public int RequestId { get; set; }
        public string Type { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string Status { get; set; }
        public string? RejectionReason { get; set; }
        public string DepartmentName { get; set; }
        public string PurposeName { get; set; }

        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case "pending": return "⏳ На проверке";
                    case "approved": return "✅ Одобрена";
                    case "rejected": return "❌ Отклонена" + (RejectionReason != null ? $": {RejectionReason}" : "");
                    default: return Status;
                }
            }
        }
    }
}