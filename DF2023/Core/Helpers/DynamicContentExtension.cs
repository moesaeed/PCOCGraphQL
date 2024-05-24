using System;
using System.Collections.Generic;
using System.Linq;
using Telerik.Sitefinity.Data.ContentLinks;
using Telerik.Sitefinity.Data.Linq.Dynamic;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Model.ContentLinks;
using Telerik.Sitefinity.Utilities.TypeConverters;

namespace DF2023.Core.Helpers
{
    public static class DynamicContentExtension
    {
        private static IQueryable<ContentLink> GetRelationsByParent(Guid parentItemId, string parentItemType, string childItemType, string fieldName, string itemProviderName = "OpenAccessProvider")
        {
            ContentLinksManager contentLinksManager = ContentLinksManager.GetManager();

            var linksToRelatedItems = contentLinksManager.GetContentLinks()
                .Where(cl => cl.ParentItemId == parentItemId &&
                    cl.ParentItemProviderName == itemProviderName &&
                    cl.ChildItemType == childItemType &&
                    cl.ParentItemType == parentItemType &&
                    cl.ComponentPropertyName == fieldName);

            return linksToRelatedItems;
        }

        private static IQueryable<ContentLink> GetRelationsByChild(Guid childItemId, string childItemType, string parentItemType, string fieldName ="", string itemProviderName = "OpenAccessProvider")
        {
            ContentLinksManager contentLinksManager = ContentLinksManager.GetManager();
            if(!string.IsNullOrWhiteSpace(fieldName))
            {
                var linksToRelatedItems = contentLinksManager.GetContentLinks()
                    .Where(cl => cl.ChildItemId == childItemId &&
                        cl.ChildItemProviderName == itemProviderName &&
                        cl.ChildItemType == childItemType &&
                        cl.ParentItemType == parentItemType &&
                        cl.ComponentPropertyName == fieldName);

                return linksToRelatedItems;
            }
            else
            {
                var linksToRelatedItems = contentLinksManager.GetContentLinks()
                .Where(cl => cl.ChildItemId == childItemId &&
                    cl.ChildItemProviderName == itemProviderName &&
                    cl.ChildItemType == childItemType &&
                    cl.ParentItemType == parentItemType);

                return linksToRelatedItems;
            }
        }

        public static DynamicContent GetMasterItem(DynamicModuleManager manager, string typeName, Guid iD)
        {
            var item = manager.GetDataItem(TypeResolutionService.ResolveType(typeName), iD);
            //if (item.OriginalContentId != Guid.Empty)
            //item = manager.Lifecycle.GetMaster(item) as DynamicContent;
            return item;
        }

        public static IQueryable<DynamicContent> FilterDataByChild(IQueryable<DynamicContent> collection, Guid childId, string childItemType, string parentItemType, string fieldName)
        {
            if (childId != Guid.Empty
                && !string.IsNullOrWhiteSpace(childItemType)
                && collection.Count() > 0
                && !string.IsNullOrWhiteSpace(parentItemType))
            {
                var manager = DynamicModuleManager.GetManager();
                var item = GetMasterItem(manager, childItemType, childId);
                if (item != null)
                {
                    var parentIds = GetRelationsByChild(childId, childItemType, parentItemType, fieldName)
                                        .Select(p => p.ParentItemId)
                                        .ToList();

                    var list = from cl in collection
                               join itemId in parentIds on cl.Id equals itemId
                               select cl;

                    return list.AsQueryable();
                }
                return null;
            }
            return collection;
        }

        public static IQueryable<DynamicContent> FilterDataByChilds(IQueryable<DynamicContent> collection, string parentItemType, List<QueryFilter> queryFilters, bool isOr)
        {
            if (collection.Count() == 0 || string.IsNullOrWhiteSpace(parentItemType) || queryFilters == null || queryFilters.Count == 0)
            {
                return collection;
            }

            var manager = DynamicModuleManager.GetManager();
            List<Guid> parentIds = new List<Guid>();
            bool firstRun = true;

            foreach (var item in queryFilters)
            {
                var childItem = GetMasterItem(manager, item.ChildItemType, item.ChildId);

                if (childItem == null)
                {
                    return null;
                }

                var data = GetRelationsByChild(item.ChildId, item.ChildItemType, parentItemType, item.FieldName)
                                .Select(p => p.ParentItemId)
                                .ToList();

                if (isOr || firstRun)
                {
                    parentIds.AddRange(data);
                }
                else
                {
                    parentIds = parentIds.Intersect(data).ToList();
                }

                firstRun = false;
            }


            parentIds = parentIds.Distinct().ToList();

            var list = from cl in collection
                       join itemId in parentIds on cl.Id equals itemId
                       select cl;
            return list;
        }
    }

    public class QueryFilter
    {
        public Guid ChildId { get; set; }
        public string ChildItemType { get; set; }
        public string FieldName { get; set; }
    }
}