using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nass.Data
{
    public class NassadContextFactory : IDesignTimeDbContextFactory<NassadContext>
    {
        public NassadContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NassadContext>();
            optionsBuilder.UseSqlServer("Server=DESKTOP-AVG95LF\\Humber;Database=nassad;Trusted_Connection=True;TrustServerCertificate=True");

            return new NassadContext(optionsBuilder.Options);
        }
    }
}
