using Microsoft.EntityFrameworkCore;
using t_project.Database;
using t_project.Models;

namespace t_project.DB.Context
{
    public class AuditoriumContext : DbContext
    {
        public DbSet<Auditorium> Auditorium { get; set; }
        public AuditoriumContext()
        {
            Database.EnsureCreated();
            Auditorium.Load();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(Config.connection, Config.version);
        }
    }
}
