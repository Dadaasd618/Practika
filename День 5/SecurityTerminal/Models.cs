using System;
using System.Collections.Generic;

namespace SecurityTerminal
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public int? DepartmentId { get; set; }
        public string? Section { get; set; }
    }
    public class Department
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; }
    }
    public class ApprovedRequest
    {
        public int RequestId { get; set; }
        public string Type { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string DepartmentName { get; set; }
        public int DepartmentId { get; set; }  // ← ДОБАВИТЬ ЭТО
        public string PurposeName { get; set; }
        public string VisitorFullName { get; set; }
        public string VisitorPassport { get; set; }
        public string VisitorPhone { get; set; }
        public DateTime? EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }

        public string StatusText
        {
            get
            {
                if (EntryTime != null && ExitTime == null)
                    return "🟢 На территории";
                if (EntryTime != null && ExitTime != null)
                    return "🔴 Покинул";
                return "⏳ Ожидает";
            }
        }
    }
}
