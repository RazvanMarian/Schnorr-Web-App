using System;
using System.Collections.Generic;
using System.Text;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.DataAccess
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Address> Addresses { get; set; }
    }
}
