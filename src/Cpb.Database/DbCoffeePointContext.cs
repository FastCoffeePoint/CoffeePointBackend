using Microsoft.EntityFrameworkCore;

namespace Cpb.Database;

public class DbCoffeePointContext(DbContextOptions<DbCoffeePointContext> options) : DbContext(options)
{
    public DbSet<DbUser> Users { get; set; }
}