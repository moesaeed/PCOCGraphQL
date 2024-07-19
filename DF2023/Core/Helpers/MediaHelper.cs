using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Telerik.Sitefinity;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.Services;

namespace DF2023.Core.Helpers
{
    public class MediaHelper
    {
        public static Image CreateImage(string title, string albumName, string base64encodedstring, Image avatarPhoto = null)
        {
            if (string.IsNullOrEmpty(base64encodedstring))
                return null;

            if (base64encodedstring.Split(',').Length == 2)
            {
                var base64String = base64encodedstring.Split(',')[1];
                var imageExtension = "." + base64encodedstring.Split(',')[0].Split('/')[1].Split(';')[0];
                var bytes = Convert.FromBase64String(base64String);
                var memoryStream = new MemoryStream(bytes);
                var album = GetOrCreateGetAlbum(albumName);
                if (album != null)
                {
                    if (avatarPhoto == null)
                    {
                        var image = CreateImage(album, title, memoryStream, imageExtension, new List<string> { "en" });
                        return image;
                    }
                    else
                    {
                        avatarPhoto = UpdateImage(album, avatarPhoto, title, memoryStream, imageExtension, new List<string> { "en" });
                        return avatarPhoto;
                    }
                }
            }
            return null;
        }

        private static Image CreateImage(Album album, string imageTitle, Stream imageStream, string imageExtension, List<string> cultures)
        {
            LibrariesManager manager = LibrariesManager.GetManager();
            manager.Provider.SuppressSecurityChecks = true;
            var imageId = Guid.NewGuid();
            var currentCulure = SystemManager.CurrentContext.Culture;
            var imageCreated = false;
            Image image = null;
            foreach (string culture in cultures)
            {
                if (!imageCreated)
                {
                    image = manager.CreateImage();
                    image.Parent = album;
                    image.Title[currentCulure] = imageTitle;
                    image.DateCreated = DateTime.UtcNow;
                    image.PublicationDate = DateTime.UtcNow;
                    image.LastModified = DateTime.UtcNow;
                    image.Owner = Telerik.Sitefinity.Security.SecurityManager.GetCurrentUserId();
                    image.Author = Telerik.Sitefinity.Security.SecurityManager.GetCurrentUserName();
                    image.UrlName[currentCulure] = Guid.NewGuid().ToString();
                    image.ApprovalWorkflowState = "Published";
                    try
                    {
                        manager.Upload(image, imageStream, imageExtension);
                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex, ConfigurationPolicy.ErrorLog);
                        return null;
                    }
                    manager.Lifecycle.Publish(image);
                    imageId = image.Id;
                    imageCreated = true;
                }
                else
                {
                    image = manager.GetImage(imageId);
                    image.Title[currentCulure] = imageTitle;
                }
            }
            manager.Provider.SuppressSecurityChecks = false;
            manager.SaveChanges();
            return image;
        }

        private static Image UpdateImage(Album album, Image image, string imageTitle, Stream imageStream, string imageExtension, List<string> cultures)
        {
            LibrariesManager manager = LibrariesManager.GetManager();
            manager.Provider.SuppressSecurityChecks = true;
            var currentCulure = SystemManager.CurrentContext.Culture;
            foreach (string culture in cultures)
            {
                image.Title[currentCulure] = imageTitle;
                image.LastModified = DateTime.UtcNow;
                image.Owner = Telerik.Sitefinity.Security.SecurityManager.GetCurrentUserId();
                image.Author = Telerik.Sitefinity.Security.SecurityManager.GetCurrentUserName();
                image.ApprovalWorkflowState = "Published";
                try
                {
                    manager.Upload(image, imageStream, imageExtension);
                }
                catch (Exception ex)
                {
                    Log.Write(ex, ConfigurationPolicy.ErrorLog);
                    return null;
                }
                manager.Lifecycle.Publish(image);
            }
            manager.Provider.SuppressSecurityChecks = false;
            return image;
        }

        private static Album GetOrCreateGetAlbum(string title)
        {
            LibrariesManager manager = LibrariesManager.GetManager();
            Album album = GetAlbumByTitle(title);
            manager.Provider.SuppressSecurityChecks = true;
            if (album == null)
            {
                album = manager.CreateAlbum();
                album.Title = title;
                album.DateCreated = DateTime.UtcNow;
                album.PublicationDate = DateTime.UtcNow;
                album.LastModified = DateTime.UtcNow;
                album.Owner = Telerik.Sitefinity.Security.SecurityManager.GetCurrentUserId();
                album.UrlName = Regex.Replace(title.ToLower(), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-");

                manager.RecompileAndValidateUrls(album);
                manager.SaveChanges();
            }
            manager.Provider.SuppressSecurityChecks = false;
            return album;
        }

        private static Album GetAlbumByTitle(string title)
        {
            LibrariesManager manager = LibrariesManager.GetManager();
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

        public static IFolder GetFolder(Guid libraryId, string folderName, string providerName, LibrariesManager mngr = null)
        {
            LibrariesManager librariesManager = mngr;
            if (librariesManager == null)
            {
                librariesManager = LibrariesManager.GetManager(providerName);
            }
            Album album = librariesManager.GetAlbums().Where(i => i.Id == libraryId).SingleOrDefault();
            var folder = librariesManager.GetChildFolders(album).FirstOrDefault(i => i.Title == folderName);
            if(folder==null)
            {
                folder= GetOrCreateFolder(providerName, folderName, album, mngr);
            }
            return folder;
        }

        public static IFolder CreateLibrary(Guid libraryId, string folderName, string providerName, LibrariesManager mngr = null)
        {
            LibrariesManager librariesManager = mngr;
            if (librariesManager == null)
            {
                librariesManager = LibrariesManager.GetManager(providerName);
            }
            Album album = librariesManager.GetAlbums().Where(i => i.Id == libraryId).SingleOrDefault();
            var folder = GetOrCreateFolder(providerName, folderName, album, mngr);
            return folder;
        }
        public static IFolder GetOrCreateFolder(string providerName, string folderUrlName, IFolder parentFolder, LibrariesManager mngr = null)
        {
            LibrariesManager librariesManager = mngr;
            if (librariesManager == null)
            {
                librariesManager = LibrariesManager.GetManager(providerName);
            }
            var siteFolder = librariesManager.GetChildFolders(parentFolder).Where(x => x.Title == folderUrlName).FirstOrDefault();
            if (siteFolder == null)
            {
                Guid folderId = Guid.NewGuid();
                var folder = librariesManager.CreateFolder(folderId, parentFolder);
                folder.Title = folderUrlName;
                folder.LastModified = DateTime.UtcNow;
                folder.UrlName = Regex.Replace(folderUrlName.ToLower(), @"[^\w\-\!\$\'\(\)\=\@\d_]+", "-");
                if (mngr == null)
                {
                    librariesManager.SaveChanges();
                }
                return folder;
            }
            return siteFolder;
        }
    }
}