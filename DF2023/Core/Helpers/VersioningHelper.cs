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
using Telerik.Sitefinity.Versioning.Model;
using Telerik.Sitefinity;
using Telerik.Sitefinity.RelatedData;

namespace DF2023.Core.Helpers
{
    public class VersioningHelper
    {
        public static List<CompareResult> GetDiff(string itemType, Guid id)
        {
            var versionHistory = GetItemVersionHistory(itemType, id,true);
            var changes = versionHistory.Changes;
            var item = versionHistory.item;

            if (changes.Count < 2) return null;
            if (changes[0] == null) return null;
            if (changes[1] == null) return null;

            var jsonLastPublished = GetData(changes[0]);
            var jsonPreviousChanged = GetData(changes[1]);

            var fieldsToCompare = GetVisibleDynamicContentFields(item)
                .Where(f => !(f.FieldType == FieldType.RelatedData || f.FieldType == FieldType.RelatedMedia || f.FieldType == FieldType.Choices))
                .ToList();
            var relatedFieldNames = GetRelatedDataDynamicContentFields(item);

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

        public static List<HChanges> GetFullHistory(string itemType, Guid id)
        {
            var versionHistory = GetItemVersionHistory(itemType, id);
            var changes = versionHistory.Changes;
            var item = versionHistory.item;
            if (changes.Count < 2) return null;
            var fieldsToCompare = GetVisibleDynamicContentFields(item)
                .Where(f => !(f.FieldType == FieldType.RelatedData || f.FieldType == FieldType.RelatedMedia || f.FieldType == FieldType.Choices))
                .ToList();
            var relatedFieldNames = GetRelatedDataDynamicContentFields(item);

            List<HChanges> finalResult = new List<HChanges>();
            for (int i = 0; i<changes.Count -1;i++)
            {
                HChanges ch = new HChanges
                {
                    version = changes[i].Version.ToString(),
                    user = changes[i].GetUserDisplayName(),
                    modified = changes[i].LastModified.ToString()
                };

                List<CompareResult> result = new List<CompareResult>();
                var jsonLast = GetData(changes[i+1]);
                var jsonPrevious= GetData(changes[i]);
                if (jsonPrevious!= null && jsonLast != null)
                {
                    foreach (var regularField in fieldsToCompare)
                    {
                        result.Add(CompareSimpleField(regularField, jsonLast, jsonPrevious, regularField.FieldType));
                    }
                    foreach (var relatedField in relatedFieldNames)
                    {
                        result.Add(CompareRelatedItemFields(relatedField, jsonLast, jsonPrevious));
                    }
                    ch.results = result.Where(r => r.AreDifferent).ToList();
                    if(ch.results.Count > 0)
                        finalResult.Add(ch);
                }
            }
            return finalResult;
        }

        private static (DynamicContent item, List<Change> Changes) GetItemVersionHistory(string itemType, Guid id, bool isDescending = false)
        {
            DynamicModuleManager manager = new DynamicModuleManager();
            VersionManager versionManager = VersionManager.GetManager();
            var typeResolved = TypeResolutionService.ResolveType(itemType);
            var item = manager.GetDataItem(typeResolved, id);

            var changes = versionManager
                .GetItemVersionHistory(id).ToList();
            if(isDescending)
            {
                changes = changes.OrderByDescending(itm => itm.Version).ToList();
            }
            else
            {
                changes = changes.OrderBy(itm => itm.Version).ToList();
            }
            return (item, changes);
        }

        private static JObject GetData(Change changeItem)
        {
            MyBinarySerializer serializer = new MyBinarySerializer();
            serializer.helper = new BinaryHelper(new MemoryStream(changeItem.Data), System.Text.Encoding.UTF8);
            var jsonChanges = serializer.helper.GetPlainJson();
            return jsonChanges;
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
            JArray previousChangedArray = (JArray)jsonPreviousChanged[relatedField.Name];

            result.NewValue = string.Join(",", lastPublishedArray.OrderBy(itm => itm["Id"].ToString()).Select(itm => itm["Id"].ToString()));
            result.OldValue = string.Join(",", previousChangedArray.OrderBy(itm => itm["Id"].ToString()).Select(itm => itm["Id"].ToString()));
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

            var valOld = (string)Convert.ChangeType(jsonPreviousChanged[regularField.Name], typeof(string));
            var valNew = (string)Convert.ChangeType(jsonLastPublished[regularField.Name], typeof(string));

            if (fieldtype == FieldType.DateTime)
            {
                if (jsonPreviousChanged[regularField.Name] != null && !string.IsNullOrWhiteSpace(jsonPreviousChanged[regularField.Name].ToString()))
                {
                    var vlues = (DateTime)Convert.ChangeType(jsonPreviousChanged[regularField.Name], typeof(DateTime));
                    valOld = vlues.ToString("yyyy-MM-ddTHH:mm:ss.fff");
                }
                if (jsonLastPublished[regularField.Name] != null && !string.IsNullOrWhiteSpace(jsonLastPublished[regularField.Name].ToString()))
                {
                    var vlues = (DateTime)Convert.ChangeType(jsonLastPublished[regularField.Name], typeof(DateTime));
                    valNew = vlues.ToString("yyyy-MM-ddTHH:mm:ss.fff");
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

    public class HChanges
    {
        public string version { get; set; }
        public string modified { get; set; }
        public string user { get; set; }
        public List<CompareResult> results { get; set; }
    }
}