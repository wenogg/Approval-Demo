using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApprovalDemo.Permissions;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;

namespace ApprovalDemo.DataSeeders;

public class RoleDataSeeder(RoleManager<IdentityRole> roleManager, IPermissionManager permissionManager)
        : IDataSeedContributor, ITransientDependency
    {
        private const string Buyer = "Buyer";
        private const string Packager = "Packager";
        private const string Shipper = "Shipper";

        private static readonly Dictionary<string, List<string>> RolesAndPermissions = new()
        {
            [Buyer] = [
                ApprovalDemoPermissions.Orders.Default,
                ApprovalDemoPermissions.Orders.Modify,
                ApprovalDemoPermissions.Orders.Receive
            ],
            [Packager] = [
                ApprovalDemoPermissions.Orders.Default,
                ApprovalDemoPermissions.Orders.Prepare
            ],
            [Shipper] = [
                ApprovalDemoPermissions.Orders.Default,
                ApprovalDemoPermissions.Orders.Ship
            ],
        };

        public async Task SeedAsync(DataSeedContext context)
        {
            foreach (var roleWithPermissions in RolesAndPermissions)
            {
                var roleName = roleWithPermissions.Key;
                var role = await EnsureRoleExists(roleName);
                foreach (var permission in roleWithPermissions.Value)
                {
                    await EnsureRoleHasPermission(role, permission);
                }
            }
        }

        private async Task EnsureRoleHasPermission(IdentityRole role, string permission)
        {
            var existingPermission = await permissionManager.GetForRoleAsync(role.Name, permission);
            if (existingPermission == null || !existingPermission.IsGranted)
            {
                await permissionManager.SetForRoleAsync(role.Name, permission, true);
            }
        }

        private async Task<IdentityRole> EnsureRoleExists(string roleName)
        {
            var role = await roleManager.FindByNameAsync(roleName);

            if (role == null)
            {
                role = new IdentityRole(Guid.NewGuid(), roleName)
                {
                    IsStatic = true
                };
                await roleManager.CreateAsync(role);
            }

            return role;
        }
    }