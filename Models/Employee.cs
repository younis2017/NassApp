namespace Nass.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal AnnualSalary { get; set; }
        public bool IsManager { get; set; }
        public int DepartmentId { get; set; }

        // Navigation property by convention virtual to Department and Creat a 1-M relationship
        public virtual Department Department { get; set; } = null!;



    }
}
