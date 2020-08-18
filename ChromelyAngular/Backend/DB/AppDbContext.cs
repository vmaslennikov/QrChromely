using System;
using System.IO;

using ChromelyAngular.Backend.Models;

using Microsoft.EntityFrameworkCore;

namespace ChromelyAngular.Backend.DB
{
    public class AppDbContext : DbContext
    {
        public DbSet<Person> Persons { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var source = Path.Combine(Environment.CurrentDirectory, "qr.db");
            optionsBuilder.UseSqlite($"DataSource={source};");
        }
    }
}
