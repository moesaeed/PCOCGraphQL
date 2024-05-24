using System;
using System.Collections.Generic;

namespace DF2023.GraphQL.Classes
{
    public class MetaClass
    {
    }

    public class MetaTypeModel
    {
        public Guid Id { get; set; }
        public string Namespace { get; set; }
        public List<MetaFieldModel> Fields { get; set; } = new List<MetaFieldModel>();
        public string ClassName { get; set; }
        public Type ClrType { get; set; }

        public string FullTypeName { get; set; }

        public Guid ParentTypeId { get; set; }
    }

    public class MetaFieldModel
    {
        public Guid Id { get; set; }

        public string ClrType { get; set; }

        public string FieldName { get; set; }

        public string Description { get; set; }

        public MetaTypeModel Parent { get; set; }
        public List<MetaAttributeModel> MetaAttributes { get; set; } = new List<MetaAttributeModel>();
    }

    public class MetaAttributeModel
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }
}