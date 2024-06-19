using DF2023.WebPageHelper;
using IdentityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.RelatedData;
using Telerik.Sitefinity.Utilities.TypeConverters;

namespace DF2023.WebPages
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Conventions.DataSource = GetDataHelper.GetConventionsForDropDown(GetBaseUrl());
                Conventions.DataTextField = "Title";
                Conventions.DataValueField = "Id";
                Conventions.DataBind();
            }
        }

        protected void btnRelateDelegationToGuests_Click(object sender, EventArgs e)
        {
            string baseUrl = GetBaseUrl();
            Guid conventionId = Guid.Parse(Conventions.SelectedValue);
            //Load All delegation
            var manager = DynamicModuleManager.GetManager();
            var delegations = manager.GetDataItems(TypeResolutionService.ResolveType(Core.Constants.Delegation.DelegationDynamicTypeName))
                                      .Where(i => i.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Live && i.Visible &&
                                                  i.SystemParentId == conventionId)
                                      .ToList();
            var guests = manager.GetDataItems(TypeResolutionService.ResolveType(Core.Constants.Guest.GuestDynamicTypeName))
                          .Where(i => i.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Live && i.Visible &&
                                      i.SystemParentId == conventionId)
                          .ToList();

            foreach(DynamicContent delegation  in delegations)
            {
                var masterDelegation = manager.Lifecycle.GetMaster(delegation) as DynamicContent;
                bool isSingle = delegation.GetValue<bool>(Core.Constants.Delegation.IsSingle);
                int randomGuests = 1;
                if (!isSingle)
                {
                    randomGuests = new Random().Next(1, 3);
                }

                for (int i = 0; i < randomGuests; i++)
                {
                    if (guests.Count() == 0) break;
                    var guestIndex = new Random().Next(0, guests.Count());

                    masterDelegation.CreateRelation(guests[guestIndex], Core.Constants.Delegation.Guests);
                    guests.RemoveAt(guestIndex);
                    guests.TrimExcess();
                   // manager.SaveChanges();
                }
            }

           manager.SaveChanges();
        }
        private string GetBaseUrl()
        {
            var requestUrl = HttpContext.Current?.Request?.Url;
            string baseUrl = $"{requestUrl.Scheme}://{requestUrl.Host}:{requestUrl.Port}/";
            return baseUrl;
        }
    }
}