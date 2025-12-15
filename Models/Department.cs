namespace Nass.Models
{
    public class Department
	{
        public int Id { get; set; }
        public string ShortName { get; set; }
        public string LongName { get; set; }

        // Navigation to many employees 

        public virtual List<Employee> Employees { get; set; } = new(); //1-M relationship
        // to allow EF Core to initialize the collection and avoid null reference exceptions
    }

}
