using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.DataAccess
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions options) : base(options) { }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Key> Keys { get; set; }
        public DbSet<Company> Companies { get; set; }
    }
}
