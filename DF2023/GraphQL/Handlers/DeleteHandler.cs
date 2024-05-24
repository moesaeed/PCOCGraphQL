using GraphQL;
using System;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.Utilities.TypeConverters;

namespace DF2023.GraphQL.Handlers
{
    public class DeleteHandler
    {
        public static object HandleDelete(IResolveFieldContext context, string fullTypeName)
        {
            var typeResolved = TypeResolutionService.ResolveType(fullTypeName);
            var dynamicManager = DynamicModuleManager.GetManager();
            var id = context.Arguments.ContainsKey("id") ? Guid.Parse(context.Arguments["id"].Value.ToString()) : Guid.Empty;
            if (id == Guid.Empty) return null;
            var item = dynamicManager.GetDataItem(typeResolved, id);

            dynamicManager.RecycleBin.MoveToRecycleBin(item);
            dynamicManager.SaveChanges();

            return item;
        }
    }
}