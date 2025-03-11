using Microsoft.EntityFrameworkCore;
using t_project.Database;
using t_project.Models;

namespace t_project.DB.Context
{
    public class StatusContext : DbContext
    {
        public DbSet<Status> Status { get; set; }
        public StatusContext()
        {
            Database.EnsureCreated();
            Status.Load();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(Config.connection, Config.version);
        }
    }
}
