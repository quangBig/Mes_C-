using System;
using MES.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MES.API.Data
{
    public class MESDbContext : DbContext
    {
        public MESDbContext(DbContextOptions<MESDbContext> options)
        : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Seed roles
            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = 1,
                    RoleName = "Admin"
                },
                new Role
                {
                    Id = 2,
                    RoleName = "Manager"
                },
                new Role
                {
                    Id = 3,
                    RoleName = "Engineer"
                },
                new Role
                {
                    Id = 4,
                    RoleName = "Operator"
                },
                new Role
                {
                    Id = 5,
                    RoleName = "QC"
                }
            );

            // seed tài khoản admin mặc định để đăng nhập
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    UserName = "admin",
                    Email = "wuangbig204@gmail.com",
                    PasswordHash = "admin",
                    RoleId = 1
                }
            );
        }
    }
}
