using System;
using System.Collections.Generic;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Security;
using Telerik.Sitefinity.Security.Model;

namespace DF2023.Core.Extensions
{
    public static class RoleExtensions
    {
        public static bool AddUserToRoles(User user, List<string> rolesToAdd, string transaction)
        {
            try
            {
                RoleManager appRoleManager = RoleManager.GetManager(SecurityConstants.ApplicationRolesProviderName, transaction);
                RoleManager roleManager = RoleManager.GetManager("", transaction);
                appRoleManager.Provider.SuppressSecurityChecks = true;
                roleManager.Provider.SuppressSecurityChecks = true;

                foreach (var roleName in rolesToAdd)
                {
                    if (appRoleManager.RoleExists(roleName))
                    {
                        Role role = appRoleManager.GetRole(roleName);
                        appRoleManager.AddUserToRole(user, role);
                    }
                    else if (roleManager.RoleExists(roleName))
                    {
                        Role role = roleManager.GetRole(roleName);
                        roleManager.AddUserToRole(user, role);
                    }
                }

                appRoleManager.Provider.SuppressSecurityChecks = false;
                roleManager.Provider.SuppressSecurityChecks = false;

                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex, ConfigurationPolicy.ABTestingTrace);
                return false;
            }
        }
    }
}