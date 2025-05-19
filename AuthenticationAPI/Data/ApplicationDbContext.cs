using AuthenticationAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationAPI.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options):base(options)
        {

        }
        public DbSet<Users> User{ get; set; }
        public DbSet<Roles> Role{ get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //seed roles
            modelBuilder.Entity<Roles>().HasData(
                new Roles {RoleId=1,RoleName="Admin" },
                new Roles {RoleId=2,RoleName="User" }
                );

            modelBuilder.Entity<Users>()
                .HasOne(u => u.Role).WithMany(r => r.User).HasForeignKey(u => u.RoleID);


        }

      

    }
}
