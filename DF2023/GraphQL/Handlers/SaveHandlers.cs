using DF2023.Core.Constants;
using DF2023.Core.Custom;
using DF2023.Core.Extensions;
using DF2023.Core.Helpers;
using DF2023.GraphQL.Classes;
using DF2023.Mvc.Models;
using GraphQL;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Telerik.Sitefinity;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Lifecycle;
using Telerik.Sitefinity.Localization;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.RelatedData;
using Telerik.Sitefinity.Utilities.TypeConverters;
using Telerik.Sitefinity.Versioning;
using Telerik.Sitefinity.Workflow;

namespace DF2023.GraphQL.Handlers
{
    public class SaveHandlers
    {
        public static object HandleSave(IResolveFieldContext context, string fullTypeName)
        {
            var userId = UserExtensions.GetCurentUserId();
            if (userId == Guid.Empty)
            {
                return null;
            }

            Dictionary<string, Object> args = context.Arguments.Values.FirstOrDefault().Value as Dictionary<string, Object>;
            var mappedManager = ManagerBase.GetMappedManager(fullTypeName);
            if (mappedManager is DynamicModuleManager)
                return HandleDynamicContentItemCreation(mappedManager, fullTypeName, args);
            else if (mappedManager is LibrariesManager)
                return HandleLibraryItem(mappedManager, fullTypeName, args);
            return null;
        }

        private static MediaContent HandleLibraryItem(IManager mappedManager, string fullTypeName, Dictionary<string, Object> contextValue)
        {
            var librariesManager = (LibrariesManager)mappedManager;
            var id = contextValue.ContainsKey("id") ? Guid.Parse(contextValue["id"].ToString()) : Guid.Empty;
            var title = contextValue.ContainsKey("title") ? contextValue["title"]?.ToString() : string.Empty;
            var alt = contextValue.ContainsKey("alt") ? contextValue["alt"]?.ToString() : string.Empty;
            var base64encodedstring = contextValue.ContainsKey("base64Content") ? contextValue["base64Content"]?.ToString() : string.Empty;
            MediaContent item = null;

            if (!string.IsNullOrWhiteSpace(base64encodedstring) && base64encodedstring.Split(',').Length == 2)
            {
                var base64String = base64encodedstring.Split(',')[1];
                var imageExtension = "." + base64encodedstring.Split(',')[0].Split('/')[1].Split(';')[0];
                byte[] imageArray = Convert.FromBase64String(base64String);
                if (id == Guid.Empty)
                {
                    if (fullTypeName == "Telerik.Sitefinity.Libraries.Model.Image")
                        item = librariesManager.CreateImage();
                    else
                        item = librariesManager.CreateDocument();
                }
                else
                {
                    if (fullTypeName == "Telerik.Sitefinity.Libraries.Model.Image")
                        item = librariesManager.GetImages().FirstOrDefault(i => i.Id == id);
                    else
                        item = librariesManager.GetDocuments().FirstOrDefault(i => i.Id == id);
                }

                if (fullTypeName == "Telerik.Sitefinity.Libraries.Model.Image")
                {
                    var parentLibrary = LibrariesManager.GetManager().GetAlbums().FirstOrDefault();
                    item.Parent = parentLibrary;
                    item.Title = title;
                }
                else
                {
                    var parentDocument = LibrariesManager.GetManager().GetDocumentLibraries().FirstOrDefault();
                    item.Parent = parentDocument;
                    item.Title = title;
                }

                using (MemoryStream ms = new MemoryStream(imageArray))
                {
                    librariesManager.Upload(item, ms, imageExtension);
                }

                if (id == Guid.Empty)
                {
                    item.SetString("UrlName", $"{StringHelper.SetValidUrlName(title)}-{Guid.NewGuid()}");
                }

                item.ApprovalWorkflowState = ApprovalStatusConstants.Published;
                ILifecycleDataItem publishedItem = librariesManager.Lifecycle.Publish(item);
                item.SetWorkflowStatus(librariesManager.Provider.ApplicationName, "Published");

                librariesManager.SaveChanges();
            }
            return item;
        }

        private static DynamicContent HandleDynamicContentItemCreation(IManager manager, string contentType, Dictionary<string, Object> contextValue)
        {
            using (CultureRegion cr = new CultureRegion(new CultureInfo("en")))
            {
                var id = contextValue.ContainsKey("id") ? Guid.Parse(contextValue["id"].ToString()) : Guid.Empty;
                var title = contextValue.ContainsKey("title") ? contextValue["title"].ToString() : string.Empty;
                var type = TypeResolutionService.ResolveType(contentType);

                ContentHandler handler = ContentHandlerFactory.GetHandler(contentType);

                // Validation before any processing
                string errorMsg;
                if (!handler.IsDataValid(contextValue, out errorMsg))
                {
                    throw new NoStackTraceException(errorMsg);
                }

                // Pre-process data before setting value
                handler.PreProcessData(contextValue);

                if (!string.IsNullOrWhiteSpace(handler.TitleValue))
                {
                    contextValue["title"] = handler.TitleValue;
                }

                var metaType = FieldHandlers.SitefinityMetaTypes.FirstOrDefault(t => t.Namespace == type.Namespace && t.ClassName == type.Name);
                var dynamicManager = DynamicModuleManager.GetManager();

                DynamicContent item = null;
                if (id == Guid.Empty)
                    item = dynamicManager.CreateDataItem(type);
                else
                    item = dynamicManager.GetDataItems(type).FirstOrDefault(i => i.Id == id);

                handler.DuringProcessData(item, contextValue);

                List<DynamicContent> oldrelatedItems = null;
                foreach (var field in contextValue.Where(f => !f.Key.ToLower().StartsWith("child")))
                {
                    string normalizedFieldName = field.Key[0].ToInvariantUpper() + field.Key.Sub(1, field.Key.Length - 1);
                    var f = metaType.Fields.FirstOrDefault(p => p.FieldName == normalizedFieldName);
                    if (f != null && f.ClrType == typeof(RelatedItems).FullName)
                    {
                        item.DeleteRelations(normalizedFieldName);
                        RelateItem(item, f, field, normalizedFieldName);
                    }
                    else
                    {
                        item.SetValue(normalizedFieldName, field.Value);
                    }
                }

                // Deal with child items.
                foreach (var childField in contextValue.Where(f => f.Key.ToLower().StartsWith("child")))
                {
                    var clrName = StringHelper.UpperFirstLetter(childField.Key.Replace("child", ""));
                    var childTypeName = FieldHandlers.SitefinityMetaTypes.FirstOrDefault(t => t.ClassName == clrName && t.Namespace == type.Namespace);
                    foreach (var childArgs in (object[])childField.Value)
                    {
                        var createdItem = HandleDynamicContentItemCreation(manager, childTypeName.FullTypeName, childArgs as Dictionary<string, object>);
                        createdItem.SystemParentId = item.Id;
                        manager.SaveChanges();
                    }
                }

                if (id == Guid.Empty)
                {
                    item.SetString("UrlName", $"{StringHelper.SetValidUrlName(title)}-{Guid.NewGuid()}");
                }

                item.ApprovalWorkflowState = ApprovalStatusConstants.Published;
                ILifecycleDataItem publishedItem = dynamicManager.Lifecycle.Publish(item);
                item.SetWorkflowStatus(dynamicManager.Provider.ApplicationName, "Published");

                //SystemManager.RunWithElevatedPrivilege(d =>
                //{
                //    manager.SaveChanges();
                //});

                manager.SaveChanges();

                var versionManager = VersionManager.GetManager();
                var version = versionManager.CreateVersion(item, true);
                versionManager.SaveChanges();

                // Post-process data after saving
                handler.PostProcessData(item, contextValue);

                return item;
            }
        }

        private static void RelateItem(DynamicContent item, MetaFieldModel f, KeyValuePair<string, object> field, string normalizedFieldName)
        {
            var newDicrionaryList = field.Value as object[];
            foreach (var newObject in newDicrionaryList)
            {
                var relatedFieldType = f.MetaAttributes.FirstOrDefault(metaAttribyte => metaAttribyte.Name == "RelatedType")?.Value;
                var innerManager = ManagerBase.GetMappedManager(relatedFieldType);
                IDataItem toRelate = null;
                if (innerManager is DynamicModuleManager)
                {
                    var newObjectAsDictionary = newObject.ToObjectDictionary();

                    if (item.GetType().ToString() == Delegation.DelegationDynamicTypeName
                        && normalizedFieldName == Delegation.Guests
                        && relatedFieldType == Guest.GuestDynamicTypeName)
                    {
                        //set up json
                        newObjectAsDictionary[Guest.GuestJSON] = item.Id;
                    }

                    if (ContentPermissionValidator.SkipCreatingContent(relatedFieldType))
                    {
                        var id = newObjectAsDictionary.ContainsKey("id") ? Guid.Parse(newObjectAsDictionary["id"].ToString()) : Guid.Empty;
                        if (id != Guid.Empty && !string.IsNullOrWhiteSpace(relatedFieldType))
                        {
                            var type = TypeResolutionService.ResolveType(relatedFieldType);
                            var dynamicManager = DynamicModuleManager.GetManager();
                            toRelate = dynamicManager.GetDataItems(type).FirstOrDefault(i => i.Id == id);
                        }
                    }
                    else
                    {
                        toRelate = HandleDynamicContentItemCreation(innerManager, relatedFieldType, newObjectAsDictionary);
                    }
                }
                else
                    toRelate = HandleLibraryItem(innerManager, relatedFieldType, newObject.ToObjectDictionary());

                if (toRelate != null)
                {
                    item.CreateRelation(toRelate, normalizedFieldName);
                }
            }
        }
    }
}