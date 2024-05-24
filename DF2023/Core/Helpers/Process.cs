using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Telerik.Sitefinity;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Lifecycle;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.Versioning;
using Telerik.Sitefinity.Workflow;

namespace DF2023.Core.Helpers
{
    public static class Process
    {
        public static void PublishItem(DynamicContent item, DynamicModuleManager dynamicModuleManager, string transactionName = null)
        {
            var versionManager = VersionManager.GetManager(null, transactionName);

            item.ApprovalWorkflowState = ApprovalStatusConstants.Published;
            ILifecycleDataItem publishedItem = dynamicModuleManager.Lifecycle.Publish(item);
            item.SetWorkflowStatus(dynamicModuleManager.Provider.ApplicationName, "Published");
            versionManager.CreateVersion(item, true);

            if (transactionName != null)
                try
                {
                    TransactionManager.CommitTransaction(transactionName);
                }
                catch (Exception ex)
                {
                    Log.Write(ex.ToString(), ConfigurationPolicy.ABTestingTrace);
                    TransactionManager.RollbackTransaction(transactionName);
                }
        }

        public static void SetCulture(this string cultureName)
        {
            cultureName = cultureName == "ar" ? "ar-QA" : cultureName;
            SystemManager.CurrentContext.Culture = new CultureInfo(cultureName);
        }

        public static Image CreateImage(string title, string albumName, byte[] imageData, string transactionName)
        {
            if (imageData.Length == 0)
            {
                return null;
            }

            var memoryStream = new MemoryStream(imageData);
            var album = GetOrCreateGetAlbum(albumName, transactionName);
            if (album != null)
            {
                var image = CreateImage(album, title, memoryStream, ".png", transactionName);
                return image;
            }

            return null;
        }

        private static Image CreateImage(Album album, string imageTitle, Stream imageStream, string imageExtension, string transactionName)
        {
            LibrariesManager manager = LibrariesManager.GetManager(null, transactionName);
            manager.Provider.SuppressSecurityChecks = true;

            Image image = null;

            image = manager.CreateImage();
            image.Parent = album;
            image.Title = imageTitle;
            image.ApprovalWorkflowState = "Published";
            try
            {
                manager.Upload(image, imageStream, imageExtension);
            }
            catch (Exception ex)
            {
                Log.Write(ex, ConfigurationPolicy.ABTestingTrace);
                return null;
            }
            manager.Lifecycle.Publish(image);

            manager.Provider.SuppressSecurityChecks = false;

            return image;
        }

        private static Album GetOrCreateGetAlbum(string title, string transactionName)
        {
            LibrariesManager manager = LibrariesManager.GetManager(null, transactionName);
            Album album = GetAlbumByTitle(title, transactionName);
            manager.Provider.SuppressSecurityChecks = true;
            if (album == null)
            {
                album = manager.CreateAlbum();
                album.Title = title;

                manager.RecompileAndValidateUrls(album);

                if (string.IsNullOrEmpty(transactionName))
                    manager.SaveChanges();
            }
            manager.Provider.SuppressSecurityChecks = false;
            return album;
        }

        private static Album GetAlbumByTitle(string title, string transactionName)
        {
            LibrariesManager manager = LibrariesManager.GetManager(null, transactionName);
            manager.Provider.SuppressSecurityChecks = true;
            Album item = manager.GetAlbums().Where(i => i.Title.ToString() == title).FirstOrDefault();
            if (item == null)
            {
                foreach (var album in manager.GetAlbums().ToList())
                {
                    if (album.Title.ToString() == title)
                    {
                        return album;
                    }
                }
            }
            manager.Provider.SuppressSecurityChecks = false;
            return item;
        }
    }
}
