using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Telerik.Sitefinity.Data.Metadata;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.RelatedData;
using ServiceStack;
using System.Text;

namespace DF2023.Versioning
{
    public class MyBinarySerializer
    {
        public BinaryHelper helper;
        public CultureInfo PersistSpecificCulture
        {
            get;
            set;
        }

        public MyBinarySerializer()
        {
        }

        public void Deserialize(byte[] serializedObject, ref object graph)
        {
            this.Deserialize(new MemoryStream(serializedObject), ref graph);
        }

        public void Deserialize(Stream deserializationStream, ref object graph)
        {
            this.helper = new BinaryHelper(deserializationStream, Encoding.Unicode);
            this.InternalDeserialize(graph.GetType(), ref graph);
        }

        private void InternalDeserialize(Type objectType, ref object item)
        {
            bool flag;
            this.InternalDeserialize(objectType, ref item, out flag, null, null);
        }

        private void InternalDeserialize(Type objectType, ref object item, out bool skipItem, string key, Type parentType)
        {
            skipItem = false;
            item = this.helper.ReadJson(item);
        }

        private void InternalSerialize(object obj, Type objectType)
        {

            JObject objSimple = JObject.FromObject(new
            {

            });

            var type = MetadataManager
                .GetManager()
                .GetMetaTypes()
                .FirstOrDefault(t => t.Namespace == obj.GetType().Namespace && t.ClassName == obj.GetType().Name);

            if (type != null)
            {
                foreach (var field in type.Fields)
                {
                    if (field.ClrType == typeof(Lstring).FullName ||
                        field.ClrType == typeof(string).FullName ||
                        field.ClrType == typeof(DateTime).FullName ||
                        field.ClrType == typeof(DateTime?).FullName ||
                        field.ClrType == typeof(int).FullName ||
                        field.ClrType == typeof(int?).FullName ||
                        field.ClrType == typeof(decimal).FullName ||
                        field.ClrType == typeof(decimal?).FullName ||
                        field.ClrType == typeof(float).FullName ||
                        field.ClrType == typeof(decimal?).FullName ||
                        field.ClrType == typeof(bool).FullName ||
                        field.ClrType == typeof(bool).FullName)
                    {
                        if (field.ClrType == typeof(Lstring).FullName)
                        {
                            var stringValue = ((DynamicContent)obj).GetValue(field.FieldName)?.ToString();
                            objSimple[field.FieldName] = stringValue == null ? null : JToken.FromObject(stringValue);
                        }
                        else
                        {
                            var stringValue = ((DynamicContent)obj).GetValue(field.FieldName)?.ToString();
                            objSimple[field.FieldName] = stringValue == null ? null : JToken.FromObject(stringValue);
                        }
                    }
                    else if (field.ClrType == typeof(RelatedItems).FullName)
                    {
                        var dynamicContentOriginal = (DynamicContent)obj;
                        if (dynamicContentOriginal.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Live)
                        {
                            dynamicContentOriginal = (DynamicContent)DynamicModuleManager.GetManager()
                                .Lifecycle
                                .GetMaster(dynamicContentOriginal);
                        }

                        var items = dynamicContentOriginal.GetRelatedItems(field.FieldName);

                        objSimple[field.FieldName] = JArray.FromObject(items
                            .Cast<IDynamicFieldsContainer>()
                        .ToList()
                            .Select(itm => new
                            {
                                Title = itm.GetString("Title")?.ToString(),
                                Id = itm.GetValue("Id")
                            }));
                    }
                }

                helper.WriteJson(objSimple.ToString());
            }
        }

        public void Serialize(Stream serializationStream, object graph)
        {
            this.helper = new BinaryHelper(serializationStream, Encoding.Unicode);
            this.InternalSerialize(graph, graph.GetType());
        }
    }
}