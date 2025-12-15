using Microsoft.EntityFrameworkCore;
using Nass.Models;

namespace Nass.Data
{
    public class DbAccess : DbContext
    {
        public DbAccess(DbContextOptions<DbAccess> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }

    }
}
