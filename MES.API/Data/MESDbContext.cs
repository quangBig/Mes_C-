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
        public DbSet<Product> Products { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<SerialNumber> SerialNumbers { get; set; }
        public DbSet<Defect> Defects { get; set; }
        public DbSet<ProductionTracking> ProductionTrackings { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ràng buộc duy nhất cho SerialCode
            modelBuilder.Entity<SerialNumber>()
                .HasIndex(s => s.SerialCode)
                .IsUnique();

            // Tránh vòng lặp cascade delete giữa WorkOrder -> SerialNumber -> ProductionTracking
            modelBuilder.Entity<ProductionTracking>()
                .HasOne(pt => pt.WorkOrder)
                .WithMany()
                .HasForeignKey(pt => pt.WorkOrderId)
                .OnDelete(DeleteBehavior.NoAction);

            // Seed defects
            modelBuilder.Entity<Defect>().HasData(
                new Defect { DefectId = 1, DefectCode = "DF001", DefectName = "Missing Component" },
                new Defect { DefectId = 2, DefectCode = "DF002", DefectName = "Wrong Part" },
                new Defect { DefectId = 3, DefectCode = "DF003", DefectName = "Solder Bridge" },
                new Defect { DefectId = 4, DefectCode = "DF004", DefectName = "Tombstone" },
                new Defect { DefectId = 5, DefectCode = "DF005", DefectName = "Insufficient Solder" }
            );

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
