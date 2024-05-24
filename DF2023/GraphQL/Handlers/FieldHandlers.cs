using DF2023.GraphQL.Classes;
using GraphQLParser.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using Telerik.Sitefinity.Data.Metadata;
using Telerik.Sitefinity.Libraries.Model;

namespace DF2023.GraphQL.Handlers
{
    public class FieldHandlers
    {
        public static string GetAliasOrField(GraphQLField fieldAst)
        {
            if (fieldAst.Alias != null)
                return fieldAst.Alias.Name.StringValue;
            else
                return fieldAst.Name.StringValue;
        }

        private static List<MetaTypeModel> _sitefinityMetaTypes = null;

        public static List<MetaTypeModel> SitefinityMetaTypes
        {
            get
            {
                if (_sitefinityMetaTypes == null)
                {
                    try
                    {
                        string librariesNamespace = typeof(Document).Namespace;

                        _sitefinityMetaTypes = MetadataManager.GetManager()
                            .GetMetaTypes()
                            .Where(t => t.Namespace.Contains("Telerik.Sitefinity.DynamicTypes.Model") ||
                                    t.Namespace.Contains(librariesNamespace))
                            .ToList()
                            .Select(mt => new MetaTypeModel
                            {
                                Id = mt.Id,
                                Namespace = mt.Namespace,
                                Fields = mt.Fields
                                    .ToList()
                                    .Select(f => new MetaFieldModel()
                                    {
                                        Id = f.Id,
                                        ClrType = f.ClrType,
                                        FieldName = f.FieldName,
                                        Description = f.Description,
                                        MetaAttributes = f.MetaAttributes
                                            .ToList()
                                            .Select(ma => new MetaAttributeModel()
                                            {
                                                Name = ma.Name,
                                                Value = ma.Value
                                            })
                                            .ToList()
                                    })
                                    .ToList(),
                                ClassName = mt.ClassName,
                                ClrType = mt.ClrType,
                                FullTypeName = mt.FullTypeName,
                                ParentTypeId = mt.ParentTypeId,
                            })
                            .ToList();

                        // Attach parents.
                        _sitefinityMetaTypes.ForEach(mt =>
                        {
                            mt.Fields.ForEach(f =>
                            {
                                f.Parent = mt;
                            });
                        });
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error building schema", ex);
                    }
                }
                return _sitefinityMetaTypes;
            }
        }
    }
}