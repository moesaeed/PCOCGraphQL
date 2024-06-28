using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using Telerik.Sitefinity.DynamicModules.Builder;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Utilities.TypeConverters;
using TelerikFields = Telerik.Sitefinity.DynamicModules.Builder.Model;
using Telerik.Sitefinity.Versioning.Comparison;
using Telerik.Sitefinity.Versioning;
using Telerik.Sitefinity.DynamicModules;
using System.IO;
using DF2023.Versioning;
using Newtonsoft.Json.Linq;
using Telerik.Sitefinity.DynamicModules.Builder.Model;

namespace DF2023.Core.Helpers
{
    public class VersioningHelper
    {
        public static List<CompareResult> GetDiff(string itemType, Guid id)
        {
            DynamicModuleManager manager = new DynamicModuleManager();
            VersionManager versionManager = VersionManager.GetManager();
            var typeResolved = TypeResolutionService.ResolveType(itemType);
            var item = manager.GetDataItem(typeResolved, id);

            var changes = versionManager
                .GetItemVersionHistory(id)
                .OrderByDescending(itm => itm.Version)
                .ToList();

            if (changes.Count <= 1)
                return null;

            var fieldsToCompare = GetVisibleDynamicContentFields(item)
                .Where(f => !(f.FieldType == FieldType.RelatedData || f.FieldType == FieldType.RelatedMedia || f.FieldType == FieldType.Choices))
                .ToList();

            var relatedFieldNames = GetRelatedDataDynamicContentFields(item);

            var lastedPublished = changes.Where(x => x.IsLastPublishedVersion).FirstOrDefault();
            if (lastedPublished == null) return null;

            MyBinarySerializer serializer = new MyBinarySerializer();
            serializer.helper = new BinaryHelper(new MemoryStream(lastedPublished.Data), System.Text.Encoding.UTF8);
            var jsonLastPublished = serializer.helper.GetPlainJson();

            var previousChanged = changes.Where(v => v.Version == changes.Max(x => x.Version)).FirstOrDefault();
            serializer.helper = new BinaryHelper(new MemoryStream(previousChanged.Data), System.Text.Encoding.UTF8);
            var jsonPreviousChanged = serializer.helper.GetPlainJson();

            List<CompareResult> result = new List<CompareResult>();

            if (jsonPreviousChanged != null && jsonLastPublished != null)
            {
                foreach (var regularField in fieldsToCompare)
                {
                    result.Add(CompareSimpleField(regularField, jsonLastPublished, jsonPreviousChanged, regularField.FieldType));
                }

                foreach (var relatedField in relatedFieldNames)
                {
                    result.Add(CompareRelatedItemFields(relatedField, jsonLastPublished, jsonPreviousChanged));
                }

                return result
                    .Where(r => r.AreDifferent)
                    .ToList();
            }
            else
            {
                return null;
            }
        }






        private static List<DynamicModuleField> GetRelatedDataDynamicContentFields(DynamicContent item)
        {
            var result = ModuleBuilderManager
                .GetManager()
                .Provider
                .GetDynamicModuleTypes()
                .Where(f => f.TypeName == item.GetType().Name)
                .OrderBy(m => m.TypeName)
                .ToList()
                .Select(m =>
                {
                    return new
                    {
                        Fields = ModuleBuilderManager
                                .GetManager()
                                .Provider
                                .GetDynamicModuleFields()
                                .Where(f => f.ParentTypeId == m.Id &&
                                     !f.IsHiddenField && (f.FieldType == FieldType.RelatedData || f.FieldType == FieldType.RelatedMedia || f.FieldType == FieldType.Choices))
                                .ToList()
                    };
                })
                .ToList()
            .FirstOrDefault();
            return result?.Fields;
        }

        private static List<DynamicModuleField> GetVisibleDynamicContentFields(DynamicContent item)
        {
            var result = ModuleBuilderManager
                .GetManager()
                .Provider
                .GetDynamicModuleTypes()
                .Where(f => f.TypeName == item.GetType().Name)
                .OrderBy(m => m.TypeName)
                .ToList()
                .Select(m =>
                {
                    return new
                    {
                        Fields = ModuleBuilderManager
                                .GetManager()
                                .Provider
                                .GetDynamicModuleFields()
                                .Where(f => f.ParentTypeId == m.Id &&
                                     !f.IsHiddenField)
                                .ToList()
                    };
                })
                .ToList()
            .FirstOrDefault();
            return result?.Fields;
        }

        private static CompareResult CompareRelatedItemFields(DynamicModuleField relatedField, JObject jsonLastPublished, JObject jsonPreviousChanged)
        {
            var result = new CompareResult();
            result.PropertyName = relatedField.Name;

            if (!jsonLastPublished.ContainsKey(relatedField.Name))
            {
                jsonLastPublished[relatedField.Name] = JArray.FromObject(new List<JObject>());
            }

            if (!jsonPreviousChanged.ContainsKey(relatedField.Name))
            {
                jsonPreviousChanged[relatedField.Name] = JArray.FromObject(new List<JObject>());
            }

            JArray lastPublishedArray = (JArray)jsonLastPublished[relatedField.Name];
            JArray lastChangedArray = (JArray)jsonPreviousChanged[relatedField.Name];

            result.OldValue = string.Join(",", lastPublishedArray.OrderBy(itm => itm["Id"].ToString()).Select(itm => itm["Id"].ToString()));
            result.NewValue = string.Join(",", lastChangedArray.OrderBy(itm => itm["Id"].ToString()).Select(itm => itm["Id"].ToString()));
            if (result.OldValue != result.NewValue)
            {
                result.AreDifferent = true;
            }
            return result;
        }

        private static CompareResult CompareSimpleField(DynamicModuleField regularField, JObject jsonLastPublished, JObject jsonPreviousChanged, FieldType fieldtype)
        {
            var result = new CompareResult();
            result.PropertyName = regularField.Name;

            if (!jsonLastPublished.ContainsKey(regularField.Name))
            {
                jsonLastPublished[regularField.Name] = null;
            }
            if (!jsonPreviousChanged.ContainsKey(regularField.Name))
            {
                jsonPreviousChanged[regularField.Name] = null;
            }

            var valNew = (string)Convert.ChangeType(jsonPreviousChanged[regularField.Name], typeof(string));
            var valOld = (string)Convert.ChangeType(jsonLastPublished[regularField.Name], typeof(string));

            if (fieldtype == FieldType.DateTime)
            {
                if (jsonPreviousChanged[regularField.Name] != null && !string.IsNullOrWhiteSpace(jsonPreviousChanged[regularField.Name].ToString()))
                {
                    var vlues = (DateTime)Convert.ChangeType(jsonPreviousChanged[regularField.Name], typeof(DateTime));
                    valNew = vlues.ToString("yyyy-MM-ddTHH:mm:ss.fff");
                }
                if (jsonLastPublished[regularField.Name] != null && !string.IsNullOrWhiteSpace(jsonLastPublished[regularField.Name].ToString()))
                {
                    var vlues = (DateTime)Convert.ChangeType(jsonLastPublished[regularField.Name], typeof(DateTime));
                    valOld = vlues.ToString("yyyy-MM-ddTHH:mm:ss.fff");
                }
            }

            if (valNew != valOld)
            {
                result.AreDifferent = true;
                result.OldValue = valOld;
                result.NewValue = valNew;
            }

            return result;
        }

    }
}