using System;
using System.Collections.Generic;
using System.Linq;
using Authentication.Core.Enums;
using Authentication.Core.Models.Domain.Accounts;
using Authentication.Core.Models.Domain.Auditing;
using Authentication.Core.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Data
{
    public class AppContext : DbContext
    {
        public AppContext(DbContextOptions<AppContext> options) : base(options)
        {
            Setup();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserLoginAuditRecord> UserLoginAuditRecords { get; set; }

        private ICollection<User> _inMemoryUsers;
        private ICollection<RolePermission> _inMemoryRolePermission;
        private void Setup()
        {
            _inMemoryUsers = new List<User>()
            {
                new User()
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@test.com",
                    Role = Role.SuperAdmin,
                    HashedPassword = HashUtil.HashPassword("test123"),
                    IsEnabled = true
                }
            };

            _inMemoryRolePermission = new List<RolePermission>()
            {
                new ()
                {
                    Role = Role.Admin,
                    Permission = Permission.CanCreateUser,
                    IsEnabled = true
                },
                new ()
                {
                    Role = Role.Admin,
                    Permission = Permission.CanDeleteUser,
                    IsEnabled = true
                },
                new ()
                {
                    Role = Role.Admin,
                    Permission = Permission.CanListUsers,
                    IsEnabled = true
                },
                new ()
                {
                    Role = Role.Admin,
                    Permission = Permission.CanUpdateUser,
                    IsEnabled = true
                },
                new ()
                {
                    Role = Role.Admin,
                    Permission = Permission.CanViewUser,
                    IsEnabled = true
                },
                new ()
                {
                    Role = Role.Moderator,
                    Permission = Permission.CanListUsers,
                    IsEnabled = true
                },
                new ()
                {
                    Role = Role.Moderator,
                    Permission = Permission.CanViewUser,
                    IsEnabled = true
                }
            };

            SeedData();
        }

        public void SeedData()
        {
            Users.AddRange(_inMemoryUsers);
            RolePermissions.AddRange(_inMemoryRolePermission);
            SaveChanges();
        }
    }
}