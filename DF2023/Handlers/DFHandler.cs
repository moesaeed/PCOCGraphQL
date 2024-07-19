using DF2023.Core.Helpers;
using System;
using System.Web;
using Telerik.Sitefinity.Data.Events;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Model;
using DF2023.Core.Constants;

namespace DF2023.Handlers
{
    public static class DFHandler
    {
        public static void DataEventHandler(IDataEvent evt)
        {
            if (evt.Action.ToLower().Equals("new") && evt.ItemType.Name.Equals("Convention") && ((Telerik.Sitefinity.DynamicModules.Events.DynamicContentEventBase)evt).Status == "Live")
            {
                var contentType = evt.ItemType;
                var manager = ManagerBase.GetMappedManager(contentType, evt.ProviderName);
                var originalItemId = ((Telerik.Sitefinity.DynamicModules.Events.DynamicContentEventBase)evt).OriginalContentId;
                var conference = (DynamicContent)manager.GetItem(contentType, originalItemId);
                Guid libraryId = new Guid(DFConstants.Library.LibraryId);
                var folder = MediaHelper.CreateLibrary(libraryId, conference.GetValue("Title").ToString(), DFConstants.Library.ProviderName);
                HttpContext.Current.Response.AddHeader("AlbumConventionId", folder.Id.ToString());
            }
            else if (evt.Action.ToLower().Equals("new") && evt.ItemType.Name.Equals("Delegation") && ((Telerik.Sitefinity.DynamicModules.Events.DynamicContentEventBase)evt).Status == "Live")
            {
                var contentType = evt.ItemType;
                var manager = ManagerBase.GetMappedManager(contentType, evt.ProviderName);
                var originalItemId = ((Telerik.Sitefinity.DynamicModules.Events.DynamicContentEventBase)evt).OriginalContentId;
                var delegation = (DynamicContent)manager.GetItem(contentType, originalItemId);
                string conventionTitle = delegation.SystemParentItem.GetValue("Title").ToString();
                Guid libraryId = new Guid(DFConstants.Library.LibraryId);

                var parentFolder = MediaHelper.GetFolder(libraryId,conventionTitle, DFConstants.Library.ProviderName);
                var folder = MediaHelper.GetOrCreateFolder(DFConstants.Library.ProviderName, delegation.GetValue("Title").ToString(),parentFolder);
                HttpContext.Current.Response.AddHeader("AlbumDelegationId", folder.Id.ToString());

            }
        }
    }
}