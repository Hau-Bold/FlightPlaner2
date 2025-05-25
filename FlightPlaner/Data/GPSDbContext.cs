using Microsoft.EntityFrameworkCore;
using FlightPlaner.Models.Domain;

namespace FlightPlaner.Data
{
    public class GPSDbContext: DbContext
    {
        public GPSDbContext(DbContextOptions options): base(options){}

        public DbSet<GPSDb> Coordinates { get; set; }
    }
}
