using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.Models
{
    public partial class AAContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                                                      .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                                                      .AddJsonFile("appsettings.json")
                                                      .Build();

                optionsBuilder.UseSqlServer(configuration.GetConnectionString("AA"));
            }
        }
    }
}
