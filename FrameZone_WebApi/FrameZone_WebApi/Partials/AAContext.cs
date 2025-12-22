using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

        // --- 讀取資料時自動把 DateTime 標記為 UTC ---
        //      EX:2025-01-01T00:00:00Z(後面有Z)
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {

            // 定義一個轉換器：讀取資料時，自動將 DateTime 標記為 Utc
            var utcConverter = new ValueConverter<DateTime, DateTime>(
                v => v, // 存入資料庫時保持不變
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc) // 從資料庫讀取時標記為 UTC
            );

            var utcNullableConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v
            );

            // 遍歷所有的實體 (Entity)，尋找 DateTime 類型的屬性並應用轉換器
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(utcConverter);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(utcNullableConverter);
                    }
                }
            }
        }
    }
}
