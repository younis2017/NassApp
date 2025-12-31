using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nass.Data
{
    public class NassadContextFactory : IDesignTimeDbContextFactory<NassadContext>
    {
        public NassadContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NassadContext>();
            optionsBuilder.UseSqlServer("Server=db36600.public.databaseasp.net; Database=db36600; User Id=db36600; Password=Ln3+t%Z2P7#k; Encrypt=True; TrustServerCertificate=True;");
            //optionsBuilder.UseSqlServer("Server=DESKTOP-AVG95LF\\humber;Database=nassad;Encrypt=false;Trusted_Connection=True;TrustServerCertificate=False;Connection Timeout=30;");

            return new NassadContext(optionsBuilder.Options);
        }
    }
}
