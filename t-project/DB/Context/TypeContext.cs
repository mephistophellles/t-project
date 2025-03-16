using Microsoft.EntityFrameworkCore;
using t_project.Database;
using t_project.Models;

namespace t_project.DB.Context
{
    public class TypeContext : DbContext
    {
        public DbSet<EquipmentType> Type { get; set; }
        public TypeContext()
        {
            Database.EnsureCreated();
            Type.Load();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(Config.connection, Config.version);
        }
    }
}
