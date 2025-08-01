using Microsoft.EntityFrameworkCore;
using IpManager.Domain.Models.Entity;

namespace IpManager.Domain.Models.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<IPAddresses> IpAddresses {  get; set; }
        public DbSet<Countries> Countries { get; set; }
    }
}
