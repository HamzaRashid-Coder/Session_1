namespace LabInventory.Models.DTOs.Issuance
{
    public class CreateStudentIssuanceDto
    {
        public int LabId { get; set; }
        public int ItemId { get; set; }
        public int QuantityIssued { get; set; }

        public string Student1Name { get; set; }
        public string RegistrationNo1 { get; set; }
        public string? ContactNo1 { get; set; }

        public string? Student2Name { get; set; }
        public string? RegistrationNo2 { get; set; }
        public string? ContactNo2 { get; set; }

        public string? Student3Name { get; set; }
        public string? RegistrationNo3 { get; set; }

        public string? DepartmentProgram { get; set; }
        public DateOnly IssueDate { get; set; }
        public DateOnly? DueDate { get; set; }
        public string? Remarks { get; set; }
        public string? ProjectName { get; set; }
        public string? TeacherName { get; set; }

    }
}
