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
        public DbSet<Factory> Factories { get; set; }
        public DbSet<Workshop> Workshops { get; set; }
        public DbSet<ProductionLine> ProductionLines { get; set; }
        public DbSet<Machine> Machines { get; set; }
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
                    PasswordHash = "$2a$11$s0Xy/Hzdb4tj7W6FD/A6HupJbei42bMO2OlUADSww56PIPsqZTZIm",
                    RoleId = 1
                }
            );
        }
    }
}
