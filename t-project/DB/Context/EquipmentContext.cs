using Microsoft.EntityFrameworkCore;
using t_project.Database;
using t_project.Models;

namespace t_project.DB.Context
{
    public class EquipmentContext : DbContext
    {
        public DbSet<Equipment> Equipment { get; set; }
        public EquipmentContext()
        {
            Database.EnsureCreated();
            Equipment.Load();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(Config.connection, Config.version);
        }
    }
}
