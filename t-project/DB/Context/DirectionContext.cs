using Microsoft.EntityFrameworkCore;
using t_project.Database;
using t_project.Models;

namespace t_project.DB.Context
{
    public class DirectionContext : DbContext
    {
        public DbSet<Direction> Direction { get; set; }
        public DirectionContext()
        {
            Database.EnsureCreated();
            Direction.Load();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(Config.connection, Config.version);
        }
    }
}
