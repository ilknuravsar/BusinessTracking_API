using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<OutageReport> OutageReports => Set<OutageReport>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);  

            builder.Entity<OutageReport>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(r => r.Description)
                      .IsRequired()
                      .HasMaxLength(2000);

                entity.Property(r => r.Location)
                      .IsRequired()
                      .HasMaxLength(300);

          
                entity.Property(r => r.Status)
                      .HasConversion<int>();

                entity.Property(r => r.Priority)
                      .HasConversion<int>();

             
                entity.HasOne(r => r.CreatedBy)
                      .WithMany(u => u.Reports)
                      .HasForeignKey(r => r.CreatedById)
                      .OnDelete(DeleteBehavior.Restrict);

            
                entity.HasIndex(r => new { r.Location, r.CreatedAt });
            });

      
            builder.Entity<AppUser>().ToTable("Users");
            builder.Entity<IdentityRole<Guid>>().ToTable("Roles");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        }
    }
}
