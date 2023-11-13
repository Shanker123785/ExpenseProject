using Microsoft.EntityFrameworkCore;
namespace Expenses.DB
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {

        }
        public DbSet<Expense> Expenses{ get; set; }
        public DbSet<User> Users { get; set; }

     }
}
