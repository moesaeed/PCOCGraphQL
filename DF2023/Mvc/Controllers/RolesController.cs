using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Telerik.Sitefinity.Security;
using Telerik.Sitefinity.Security.Claims;
using Telerik.Sitefinity.Security.Model;

namespace DF2023.Mvc.Controllers
{
    [Authorize]
    public class RolesController : ApiController
    {
        [HttpGet]
        public object GetCurrentUserRole()
        {
            var identity = ClaimsManager.GetCurrentIdentity();
            var userName = identity.Name;

            List<string> roles = new List<string>();

            RoleManager roleManager = RoleManager.GetManager();
            UserManager userManager = UserManager.GetManager();

            bool userExists = userManager.UserExists(userName);

            if (userExists)
            {
                User user = userManager.GetUser(userName);
                roles = roleManager.GetRolesForUser(user.Id).Select(x => x.Name).ToList();
                KeyValuePair<Guid, List<string>> keyValuePair = new KeyValuePair<Guid, List<string>>(user.Id, roles);
                return keyValuePair;
            }

            return null;
        }

        [HttpGet]
        public object GetCurrentUser()
        {
            var identity = ClaimsManager.GetCurrentIdentity();
            var userID = identity.UserId;
            return userID;
        }
    }
}