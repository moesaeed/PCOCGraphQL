using DF2023.Core.Helpers;
using DF2023.GraphQL.Classes;
using GraphQL;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Telerik.OpenAccess.DataAdapter.Util;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Data.Linq.Dynamic;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.RelatedData;
using Telerik.Sitefinity.Utilities.TypeConverters;
using Telerik.Windows.Data;

namespace DF2023.GraphQL.Handlers
{
    public static class DataHandler
    {
        public static object HandleGetItems(MetaTypeModel sitefinityMetaType, Type clrType, IResolveFieldContext args)
        {
            int skip = args.Arguments != null && args.Arguments.ContainsKey("_skip") && args.Arguments["_skip"].Value != null ? (int)args.Arguments["_skip"].Value : 0;
            int take = args.Arguments != null && args.Arguments.ContainsKey("_take") && args.Arguments["_take"].Value != null ? (int)args.Arguments["_take"].Value : int.MaxValue;

            var filters = args.Arguments != null && args.Arguments.ContainsKey("_filter") && args.Arguments["_filter"].Value != null ? (Dictionary<string, object>)args.Arguments["_filter"].Value : null;
            var order = args.Arguments != null && args.Arguments.ContainsKey("_sort") && args.Arguments["_sort"].Value != null ? (Dictionary<string, object>)args.Arguments["_sort"].Value : null;
            string provider = args.Arguments != null && args.Arguments.ContainsKey("_provider") && args.Arguments["_provider"].Value != null ? (string)args.Arguments["_provider"].Value : null;
            var manager = ManagerBase.GetMappedManager(clrType);
            var dynamicManager = DynamicModuleManager.GetManager(provider);
            var items = dynamicManager.GetDataItems(clrType)
                .Where(itm => itm.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Master);

            items = DataHandler.ApplyOrder(order, items, manager, clrType, sitefinityMetaType);
            items = DataHandler.ApplyFilters(filters, items, manager, clrType, sitefinityMetaType);

            var filteredCount = items.Count();
            var uniqueName = FieldHandlers.GetAliasOrField(args.FieldAst);
            args.UserContext[uniqueName] = filteredCount;
            if (skip > 0)
            {
                items = items.Skip(skip);
            }
            if (take > 0)
            {
                items = items.Take(take);
            }
            var result = items.ToList();
            return result;
        }

        private static IQueryable<DynamicContent> ApplyOrder(Dictionary<string, object> order, IQueryable<DynamicContent> items, IManager manager, Type clrType, MetaTypeModel metaType)
        {
            if (order == null)
                return items;

            foreach (var orderItem in order)
            {
                var fieldClrName = StringHelper.UpperFirstLetter(orderItem.Key);
                var sitefinityMetaField = metaType.Fields
                    .FirstOrDefault(f => f.FieldName == fieldClrName);

                if (sitefinityMetaField != null)
                {
                    if (sitefinityMetaField.ClrType == typeof(string).FullName ||
                        sitefinityMetaField.ClrType == typeof(Lstring).FullName ||
                        sitefinityMetaField.ClrType == typeof(DateTime?).FullName ||
                        sitefinityMetaField.ClrType == typeof(DateTime).FullName ||
                        sitefinityMetaField.ClrType == typeof(Nullable<decimal>).FullName)
                    {
                        return HandleStringOrder(sitefinityMetaField, orderItem.Value.ToString(), items);
                    }
                }
                else if (orderItem.Key.ToLower() == "createdat")
                {
                    string field = "DateCreated" + " " + orderItem.Value.ToString();
                    return items.OrderBy(field);
                }
            }
            return items;
        }

        private static IQueryable<DynamicContent> ApplyFilters(Dictionary<string, object> filters, IQueryable<DynamicContent> items, IManager manager, Type clrType, MetaTypeModel metaType)
        {
            if (filters == null)
                return items;

            foreach (var filter in filters)
            {
                var fieldClrName = StringHelper.UpperFirstLetter(filter.Key);
                var sitefinityMetaField = metaType.Fields
                    .FirstOrDefault(f => f.FieldName == fieldClrName);

                if (sitefinityMetaField != null)
                {
                    if (sitefinityMetaField.ClrType == typeof(string).FullName ||
                        sitefinityMetaField.ClrType == typeof(Lstring).FullName ||
                        sitefinityMetaField.ClrType == typeof(ChoiceOption).FullName)
                    {
                        string expression = GetLinqExpressionFromGraphQLStringFilter(sitefinityMetaField, filter.Key, JObject.FromObject(filter.Value));
                        return items.Where(expression);
                    }
                    else if (sitefinityMetaField.ClrType == typeof(int).FullName ||
                            sitefinityMetaField.ClrType == typeof(decimal).FullName ||
                            sitefinityMetaField.ClrType == typeof(float).FullName)
                    {
                        string expression = GetLinqExpressionFromGraphScalarType(sitefinityMetaField, filter.Key, JObject.FromObject(filter.Value));
                        return items.Where(expression);
                    }
                    else if (sitefinityMetaField.ClrType == typeof(bool).FullName)
                    {
                        string expression = GetLinqExpressionFromGraphQLBooleanFilter(sitefinityMetaField, filter.Key, (bool)filter.Value);
                        /*bool filterValue = (bool)filter.Value;
                        if(filterValue == false)
                            return items.Where(i => i.GetValue<bool?>("IsRelatedToDirectory") == false);
                        else
                            return items.Where(i=> i.GetValue<bool?>("IsRelatedToDirectory") == true);*/
                        items= items.Where(expression);
                    }
                    else if (sitefinityMetaField.ClrType == typeof(RelatedItems).FullName)
                    {
                        var chileType = sitefinityMetaField.MetaAttributes.FirstOrDefault(x => x.Name == "RelatedType").Value;
                        var parentType = clrType.FullName;
                        JObject filterObj = JObject.FromObject(filter.Value);
                        if (filterObj.ContainsKey("_any"))
                        {
                            var ids = ((JArray)filterObj["_any"]).Select(itm => Guid.Parse(itm["id"].ToString()));
                            var id0 = ids.FirstOrDefault();
                            var itemsAsList = items.ToList();
                            items = DynamicContentExtension.FilterDataByChild(items, id0, chileType, parentType, filter.Key);
                        }
                        else if (filterObj.ContainsKey("_all"))
                        {
                            var ids = ((JArray)filterObj["_all"]).Select(itm => Guid.Parse(itm["id"].ToString()));
                            var id0 = ids.FirstOrDefault();
                            var itemsAsList = items.ToList();
                            items = DynamicContentExtension.FilterDataByChild(items, id0, chileType, parentType, string.Empty);
                        }
                    }
                }
                else if (filter.Key.ToLower() == "id")
                {
                    Guid value = Guid.Parse(filter.Value.ToString());
                    items = items.Where(itm => itm.Id == value);
                }
                else if (filter.Key.ToLower() == "systemparentid")
                {
                    Guid value = Guid.Parse(filter.Value.ToString());
                    items = items.Where(itm => itm.SystemParentId == value);
                }
                else if (filter.Key.ToLower() == "or" || filter.Key.ToLower() == "and")
                {
                    string result = GetLogicalRegionRecursive(filter.Key, filter.Value, metaType, string.Empty);
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        items = items.Where(result);
                    }
                }
            }
            return items;
        }

        private static string GetLogicalRegionRecursive(string wrapperLogicalOperator, object region, MetaTypeModel metaType, string generated = "")
        {
            object[] operators = region as object[];
            List<string> generatedQueries = new List<string>();
            if (operators != null)
            {
                foreach (var op in operators)
                {
                    var unpacked = op as Dictionary<string, object>;
                    if (unpacked != null)
                    {
                        foreach (var filter in unpacked)
                        {
                            var fieldClrName = StringHelper.UpperFirstLetter(filter.Key);
                            var sitefinityMetaField = metaType.Fields.FirstOrDefault(f => f.FieldName == fieldClrName);

                            if (sitefinityMetaField != null)
                            {
                                // Add the field.
                                if (sitefinityMetaField.ClrType == typeof(string).FullName || sitefinityMetaField.ClrType == typeof(Lstring).FullName)
                                {
                                    string expression = GetLinqExpressionFromGraphQLStringFilter(sitefinityMetaField, filter.Key, JObject.FromObject(filter.Value));
                                    if (!string.IsNullOrWhiteSpace(expression))
                                    {
                                        generatedQueries.Add(expression);
                                    }
                                }
                                else if (sitefinityMetaField.ClrType == typeof(bool).FullName)
                                {
                                    string expression = GetLinqExpressionFromGraphQLBooleanFilter(sitefinityMetaField, filter.Key, (bool)filter.Value);
                                    if (!string.IsNullOrWhiteSpace(expression))
                                    {
                                        generatedQueries.Add(expression);
                                    }
                                }
                                else if (IsOfTypeOrNullableType(sitefinityMetaField.ClrType, typeof(DateTime)) ||
                                    IsOfTypeOrNullableType(sitefinityMetaField.ClrType, typeof(int)) ||
                                    IsOfTypeOrNullableType(sitefinityMetaField.ClrType, typeof(decimal)) ||
                                    IsOfTypeOrNullableType(sitefinityMetaField.ClrType, typeof(float)))
                                {
                                    string expression = GetLinqExpressionFromGraphScalarType(sitefinityMetaField, filter.Key, JObject.FromObject(filter.Value));
                                    if (!string.IsNullOrWhiteSpace(expression))
                                    {
                                        generatedQueries.Add(expression);
                                    }
                                }
                            }
                        }
                    }
                }

                if (generatedQueries.Count > 0)
                {
                    if (wrapperLogicalOperator == "or")
                    {
                        return string.Join("||", generatedQueries);
                    }
                    else if (wrapperLogicalOperator == "and")
                    {
                        return string.Join("&&", generatedQueries);
                    }
                }
            }
            return string.Empty;
        }

        private static string GetLinqExpressionFromGraphScalarType(MetaFieldModel field, string graphQLName, JObject filter)
        {
            if (filter.Properties() != null && filter.Properties().FirstOrDefault() != null)
            {
                var isDate = IsOfTypeOrNullableType(field.ClrType, typeof(DateTime));
                string valueFinal = isDate ? $"({filter.Properties().FirstOrDefault().Value?.ToString()})" : filter.Properties().FirstOrDefault().Value?.ToString();
                switch (filter.Properties().FirstOrDefault().Name)
                {
                    case "_eq":
                        return $"{field.FieldName} == {valueFinal}";

                    case "_lte":
                        return $"{field.FieldName} <= {valueFinal}";

                    case "_lt":
                        return $"{field.FieldName} < {valueFinal}";

                    case "_gt":
                        return $"{field.FieldName} > {valueFinal}";

                    case "_gte":
                        return $"{field.FieldName} >= {valueFinal}";
                }
            }
            return string.Empty;
        }

        private static bool IsOfTypeOrNullableType(string clrType, Type type)
        {
            var typeResolved = TypeResolutionService.ResolveType(clrType);
            var nullableType = type.GetNullableType();
            return typeResolved.IsAssignableFrom(nullableType);
        }

        private static string GetLinqExpressionFromGraphQLStringFilter(MetaFieldModel field, string graphQLName, JObject filter)
        {
            if (filter.Properties() != null && filter.Properties().FirstOrDefault() != null)
            {
                switch (filter.Properties().FirstOrDefault().Name)
                {
                    case "_contains":
                        return $"{field.FieldName}.ToLower().Contains(\"{filter.Properties().FirstOrDefault().Value?.ToString()}\")";

                    case "_eq":
                        return $"{field.FieldName}.ToLower().Equals(\"{filter.Properties().FirstOrDefault().Value?.ToString()}\")";

                    case "_neq":
                        return $"!{field.FieldName}.ToLower().Equals(\"{filter.Properties().FirstOrDefault().Value?.ToString()}\")";

                    case "_endswith":
                        return $"{field.FieldName}.ToLower().EndsWith(\"{filter.Properties().FirstOrDefault().Value?.ToString()}\")";

                    case "_startswith":
                        return $"{field.FieldName}.ToLower().StartsWith(\"{filter.Properties().FirstOrDefault().Value?.ToString()}\")";
                }
            }
            return string.Empty;
        }

        private static string GetLinqExpressionFromGraphQLBooleanFilter(MetaFieldModel field, string graphQLName, bool filter)
        {
            return $" {field.FieldName} == {filter.ToString().ToLower()}";
        }

        private static IQueryable<DynamicContent> HandleStringOrder(MetaFieldModel sitefinityMetaField, string filterDirection, IQueryable<DynamicContent> items)
        {
            items = items.OrderBy(sitefinityMetaField.FieldName + " " + filterDirection);
            return items;
        }
    }
}