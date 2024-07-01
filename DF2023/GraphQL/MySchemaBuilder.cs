using DF2023.Core.Helpers;
using DF2023.GraphQL.Classes;
using DF2023.GraphQL.Handlers;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using Telerik.Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.RelatedData;
using Telerik.Sitefinity.Security;
using Telerik.Sitefinity.Security.Model;
using Telerik.Sitefinity.Utilities.TypeConverters;
using Telerik.Web.UI;

using TypesGQL = GraphQL.Types;

namespace DF2023.GraphQL
{
    public class MySchemaBuilder
    {
        private static ISchema _generatedSchema = null;

        public static ISchema GeneratedSchema
        {
            get
            {
                if (_generatedSchema == null)
                {
                    _generatedSchema = new TypesGQL.Schema();
                    new MySchemaBuilder().BuildSchema(_generatedSchema);
                }
                return _generatedSchema;
            }
        }

        public List<MetaFieldModel> fieldsToRelate = new List<MetaFieldModel>();
        public static ISchema CurrentSchema = null;
        public ObjectGraphType RootMutationType { get; set; }
        public ObjectGraphType RootQueryType { get; set; }
        public ObjectGraphType RootSubscriptionsType { get; set; }

        public static void EnsureSchema()
        {
            var ISchema = GeneratedSchema; // Force
        }

        private void BuildSchema(ISchema schema)
        {
            CurrentSchema = schema;
            RootQueryType = new ObjectGraphType()
            {
                Name = "Query",
                Description = "Type for the select queries"
            };
            RootMutationType = new ObjectGraphType()
            {
                Name = "Mutation",
                Description = "Type for the save/edit queries"
            };
            RootSubscriptionsType = new ObjectGraphType()
            {
                Name = "Subscription",
                Description = "Contains subscriptions for all content types."
            };
            InputObjectGraphType stringFilters, intFilters, floatFilters, decimalFilters, relatedFilters, dateTimeFilters;
            RegisterCommonFilters(out stringFilters, out intFilters, out floatFilters, out decimalFilters, out relatedFilters, out dateTimeFilters);
            var librariesNamespace = typeof(Document).Namespace;
            var sitefinityTypes = FieldHandlers.SitefinityMetaTypes
                .Where(t => t.Namespace.Contains("Telerik.Sitefinity.DynamicTypes.Model") ||
                            t.Namespace.Contains(librariesNamespace))
                .ToList();


            GenerateContentTypeSchema(schema,
                stringFilters,
                relatedFilters,
                dateTimeFilters,
                intFilters,
                decimalFilters,
                floatFilters,
                librariesNamespace,
                sitefinityTypes,
                RegisterDifType());

            schema.RegisterType(RootQueryType);
            schema.RegisterType(RootMutationType);
            schema.RegisterType(intFilters);
            schema.RegisterType(decimalFilters);
            schema.RegisterType(floatFilters);
            schema.RegisterType(stringFilters);
            schema.RegisterType(dateTimeFilters);
            schema.RegisterType(relatedFilters);
            schema.RegisterType(RootSubscriptionsType);

            RegisterRelatedDataFields(schema);
            RegisterChildTypeFields(schema, sitefinityTypes);

            schema.Query = RootQueryType;
            schema.Mutation = RootMutationType;
            schema.Subscription = RootSubscriptionsType;
        }

        private static void RegisterCommonFilters(out InputObjectGraphType stringFilters, out InputObjectGraphType intFilters, out InputObjectGraphType floatFilters, out InputObjectGraphType decimalFilters, out InputObjectGraphType relatedFilters, out InputObjectGraphType dateTimeFilters)
        {
            stringFilters = new InputObjectGraphType()
            {
                Name = "StringFilters"
            };
            string[] strArrays = new string[] { "_startswith", "_endswith", "_contains", "_eq", "_ne" };
            for (int i = 0; i < (int)strArrays.Length; i++)
            {
                string str = strArrays[i];
                stringFilters.AddField(new FieldType()
                {
                    Name = str,
                    ResolvedType = new StringGraphType()
                });
            }
            intFilters = new InputObjectGraphType()
            {
                Name = "IntFilters"
            };
            decimalFilters = new InputObjectGraphType()
            {
                Name = "DecimalFilters"
            };
            floatFilters = new InputObjectGraphType()
            {
                Name = "FloatFitlers"
            };
            relatedFilters = new InputObjectGraphType()
            {
                Name = "RelatedFilters"
            };
            IInputObjectGraphType tempIdFilterGraphType = new InputObjectGraphType()
            {
                Name = "relatedFilter",
                Description = "Temporary Id filter"
            };
            tempIdFilterGraphType.AddField(new FieldType()
            {
                Name = "id",
                ResolvedType = new GuidGraphType()
            });
            relatedFilters.AddField(new FieldType()
            {
                Name = "_any",
                ResolvedType = new ListGraphType(tempIdFilterGraphType)
            });
            relatedFilters.AddField(new FieldType()
            {
                Name = "_all",
                ResolvedType = new ListGraphType(tempIdFilterGraphType)
            });
            dateTimeFilters = new InputObjectGraphType()
            {
                Name = "DateTimeFilters"
            };
            string[] scalarFilters = new string[] { "_eq", "_lte", "_gte", "_lt", "_gt" };
            foreach (var scalarFilter in scalarFilters)
            {
                intFilters.Field<IntGraphType>(scalarFilter);
                decimalFilters.Field<DecimalGraphType>(scalarFilter);
                dateTimeFilters.Field<DateTimeGraphType>(scalarFilter);
                floatFilters.Field<FloatGraphType>(scalarFilter);
            }
        }

        private void RegisterRelatedDataFields(ISchema schema)
        {
            fieldsToRelate.ForEach(f =>
            {
                try
                {
                    var relatedType = f.MetaAttributes.FirstOrDefault(a => a.Name == "RelatedType").Value;
                    Type tResolved = TypeResolutionService.ResolveType(relatedType);
                    var fieldLowercased = f.FieldName[0].ToString().ToLower() + f.FieldName.Substring(1);
                    var lowerType = tResolved.Name[0].ToString().ToLower() + tResolved.Name.Substring(1);
                    var typeOfTheType = f.Parent.ClrType;
                    var typeOfTheTypeLower = typeOfTheType.Name[0].ToString().ToLower() + typeOfTheType.Name.Substring(1);
                    var graphTypeToAddFieldTo = schema.AllTypes.FirstOrDefault(t => t.Name == typeOfTheTypeLower);
                    var graphTypeOfTheRelation = schema.AllTypes.FirstOrDefault(t => t.Name == lowerType);

                    if (graphTypeToAddFieldTo != null)
                    {
                        if (graphTypeOfTheRelation is ObjectGraphType && !((ObjectGraphType)graphTypeToAddFieldTo).HasField(fieldLowercased))
                        {
                            var listType = new ListGraphType(graphTypeOfTheRelation);

                            ((ObjectGraphType)graphTypeToAddFieldTo).Field(fieldLowercased, listType, $"Related {graphTypeOfTheRelation.Name}", null, resolve: (IResolveFieldContext context) =>
                            {
                                var fieldName = context.FieldDefinition.Name[0].ToString().ToUpper() + context.FieldDefinition.Name.Substring(1);
                                return context.Source.GetRelatedItems(fieldName);
                            });
                        }
                    }
                    var argType = schema.AllTypes.FirstOrDefault(t => t.Name == lowerType + "Args") as InputObjectGraphType;
                    var saveType = schema.AllTypes.FirstOrDefault(t => t.Name == typeOfTheTypeLower + "Args") as InputObjectGraphType;
                    if (saveType != null)
                    {
                        saveType.AddField(new FieldType()
                        {
                            Name = fieldLowercased,
                            ResolvedType = new ListGraphType(argType)
                        });
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            });
        }

        private static void RegisterChildTypeFields(ISchema schema, List<MetaTypeModel> sitefinityTypes)
        {
            sitefinityTypes.Where(t => t.ParentTypeId != null && t.ParentTypeId != Guid.Empty)
                            .ForEach(typeWithParent =>
                            {
                                var parentType = sitefinityTypes.FirstOrDefault(t => t.Id == typeWithParent.ParentTypeId);
                                if (parentType != null)
                                {
                                    var parentGraphTypeName = StringHelper.LowerFirstLetter(parentType.ClassName);
                                    var childGraphName = StringHelper.LowerFirstLetter(typeWithParent.ClassName);
                                    var parentGraphType = schema.AllTypes.FirstOrDefault(t => t.Name == parentGraphTypeName);
                                    var childGrpahType = schema.AllTypes.FirstOrDefault(t => t.Name == childGraphName);
                                    if (childGraphName != null && parentGraphType != null)
                                    {
                                        ((ObjectGraphType)parentGraphType).Field($"child{typeWithParent.ClassName}",
                                            new ListGraphType(childGrpahType),
                                            $"Returns the child {typeWithParent.ClassName} items.",
                                            null,
                                            resolve: (IResolveFieldContext context) =>
                                            {
                                                return ((DynamicContent)context.Source).GetChildItems(typeWithParent.FullTypeName);
                                            });
                                    }

                                      ((ObjectGraphType)parentGraphType).Field($"child{typeWithParent.ClassName}Count",
                                            new IntGraphType(),
                                            $"Returns the child {typeWithParent.ClassName} count.",
                                            null,
                                            resolve: (IResolveFieldContext context) =>
                                            {
                                                return ((DynamicContent)context.Source).GetChildItems(typeWithParent.FullTypeName).Count();
                                            });

                                    var createArgTypeName = parentGraphTypeName + "Args";
                                    var createArgChildTypeName = childGraphName + "Args";
                                    var createArgType = schema.AllTypes.FirstOrDefault(t => t.Name == createArgTypeName);
                                    var childGraphInputType = schema.AllTypes.FirstOrDefault(t => t.Name == createArgChildTypeName);
                                    if (createArgType != null)
                                    {
                                        ((InputObjectGraphType)createArgType).AddField(new FieldType()
                                        {
                                            Name = $"child{typeWithParent.ClassName}",
                                            ResolvedType = new ListGraphType(childGraphInputType)
                                        });
                                        ((InputObjectGraphType)childGraphInputType).AddField(new FieldType()
                                        {
                                            Name = "systemParentId",
                                            ResolvedType = new GuidGraphType()
                                        });
                                    }
                                }
                            });
        }

        private ObjectGraphType RegisterDifType()
        {
            var diffType = new ObjectGraphType()
            {
                Name = "Diff"
            };

            diffType.Field<StringGraphType>("PropertyName");
            diffType.Field<StringGraphType>("OldValue");
            diffType.Field<StringGraphType>("NewValue");
            diffType.Field<StringGraphType>("DiffHtml");
            diffType.Field<BooleanGraphType>("AreDifferent");
            diffType.Field<BooleanGraphType>("IsHtmlEnchancedField");
            diffType.Field<StringGraphType>("CompareType");
            return diffType;
        }

        private void GenerateContentTypeSchema(ISchema schema, InputObjectGraphType stringFilters, InputObjectGraphType relatedFilters, InputObjectGraphType dateTimeFilters, InputObjectGraphType intFilters,
            InputObjectGraphType decimalFilters, InputObjectGraphType floatFilters, string librariesNamespace, List<MetaTypeModel> sitefinityTypes, ObjectGraphType diffType)
        {
            sitefinityTypes
                .ForEach(sitefinityType =>
                {
                    var lowCaseClassName = sitefinityType.ClassName[0].ToString().ToLower() + sitefinityType.ClassName.Substring(1);
                    var dtoType = new ObjectGraphType() { Name = $"{lowCaseClassName}", };
                    dtoType.Field<GuidGraphType>("Id");
                    dtoType.Field<StringGraphType>("action");

                    if (sitefinityType.Namespace.IndexOf(librariesNamespace) > -1)
                    {
                        dtoType.Field("MediaUrl", new StringGraphType(), "The original url of the item", resolve: (IResolveFieldContext context) =>
                        {
                            return ((MediaContent)context.Source).MediaUrl;
                        });
                        dtoType.Field("ThumnailUrl", new StringGraphType(), "The original url of the item", resolve: (IResolveFieldContext context) =>
                        {
                            return ((MediaContent)context.Source).ThumbnailUrl;
                        });
                        dtoType.Field("Title", new StringGraphType(), "The original url of the item", resolve: (IResolveFieldContext context) =>
                        {
                            return ((MediaContent)context.Source).Title?.ToString();
                        });
                    }

                    dtoType.Field("diff",
                        new ListGraphType(diffType),
                        resolve: (IResolveFieldContext context) =>
                        {
                            return VersioningHelper.GetDiff(context.Source.GetType().FullName, ((IDataItem)context.Source).Id);
                        });

                    var responseType = new ObjectGraphType() { Name = $"{lowCaseClassName}Response" };
                    responseType.Field($"{lowCaseClassName}Items", new ListGraphType(dtoType), resolve: (IResolveFieldContext context) =>
                    {
                        return ((List<DynamicContent>)context.Source);
                    });
                    responseType.Field("TotalCount", new IntGraphType(), resolve: (context) =>
                    {
                        var queryToTakeFieldsFor = Handlers.FieldHandlers.GetAliasOrField(context.Parent.FieldAst);
                        return context.UserContext[queryToTakeFieldsFor];
                    });
                    var saveType = new InputObjectGraphType() { Name = $"{lowCaseClassName}Args" };
                    saveType.AddField(new FieldType()
                    {
                        ResolvedType = new GuidGraphType(),
                        Name = $"id",
                    });
                    var filters = new InputObjectGraphType() { Name = $"{lowCaseClassName}Filters" };
                    filters.AddField(new FieldType()
                    {
                        ResolvedType = new GuidGraphType(),
                        Name = $"id",
                    });
                    filters.AddField(new FieldType()
                    {
                        ResolvedType = new GuidGraphType(),
                        Name = "systemParentId"
                    });
                    var orders = new InputObjectGraphType() { Name = $"{lowCaseClassName}Order" };
                    orders.AddField(new FieldType()
                    {
                        ResolvedType = new GuidGraphType(),
                        Name = $"id",
                    });
                    orders.AddField(new FieldType()
                    {
                        ResolvedType = new StringGraphType(),
                        Name = $"createdAt",
                    });
                    var mutationQueryArgs = new QueryArguments()
                    {
                        new QueryArgument(saveType)
                        {
                            Name = $"{lowCaseClassName}Arg",
                            Description = $"Allows you to create {sitefinityType.ClassName}"
                        }
                    };

                    QueryArguments typeQueryArgs = GetQueryArgs(filters, orders);
                    RegisterMutationFields(sitefinityType, lowCaseClassName, dtoType, mutationQueryArgs);
                    RootQueryType.Field($"{dtoType.Name}",
                        responseType,
                        $"Returns {sitefinityType.ClassName} items based on the filters provider",
                        typeQueryArgs,
                    resolve: (IResolveFieldContext context) =>
                    {
                        return DataHandler.HandleGetItems(sitefinityType, TypeResolutionService.ResolveType(sitefinityType.FullTypeName), context);
                    });

                    if (sitefinityType.ClrType.FullName == typeof(Image).FullName || sitefinityType.ClrType.FullName == typeof(Document).FullName)
                    {
                        AddMediaContentFields(saveType);
                    }
                    else
                    {
                        AddDynamicFields(sitefinityType, stringFilters, relatedFilters, dateTimeFilters, intFilters, decimalFilters, floatFilters, dtoType, saveType, filters, orders);
                    }

                    dtoType.Field("CreatedAt", new DateTimeGraphType(), "Get the current created date of the item", resolve: (IResolveFieldContext context) =>
                    {
                        DynamicContent dcItem = context.Source as DynamicContent;
                        if (dcItem != null)
                        {
                            return dcItem.DateCreated;
                        }
                        return null;
                    });

                    dtoType.Field("Username", new StringGraphType(), "The current owner of the item.", resolve: (IResolveFieldContext context) =>
                    {
                        DynamicContent dcItem = context.Source as DynamicContent;
                        if (dcItem != null)
                        {
                            var profile = Core.Extensions.UserExtensions.GetUserProfile(dcItem);
                            if (profile != null && profile.Nickname != null)
                            {
                                return profile.Nickname;
                            }
                        }
                        return null;
                    });

                    dtoType.Field("UserId", new GuidGraphType(), "The current UserId.", resolve: (IResolveFieldContext context) =>
                    {
                        DynamicContent dcItem = context.Source as DynamicContent;
                        if (dcItem != null)
                        {
                            return dcItem.Owner;
                        }

                        return null;
                    });

                    dtoType.Field("Avatar", new StringGraphType(), "The current owner of the item.", resolve: (IResolveFieldContext context) =>
                    {
                        DynamicContent dcItem = context.Source as DynamicContent;
                        if (dcItem != null)
                        {
                            return Core.Extensions.UserExtensions.GetUserAvatarURL(dcItem);
                        }

                        return null;
                    });

                    dtoType.Field("LastUpdatedAt", new DateTimeGraphType(), "Get the current created date of the item", resolve: (IResolveFieldContext context) =>
                    {
                        DynamicContent dcItem = context.Source as DynamicContent;
                        if (dcItem != null)
                        {
                            return dcItem.LastModified;
                        }
                        return null;
                    });

                    dtoType.Field("LastModifyBy", new StringGraphType(), "The owner of the latest user who updated the item", resolve: (IResolveFieldContext context) =>
                    {
                        DynamicContent dcItem = context.Source as DynamicContent;
                        if (dcItem != null)
                        {
                            UserManager userManager = new UserManager();
                            UserProfileManager userProfileManager = new UserProfileManager();
                            var userId = dcItem.LastModifiedBy;
                            if (userId != Guid.Empty)
                            {
                                var user = userManager.GetUser(dcItem.LastModifiedBy);
                                var profile = userProfileManager.GetUserProfile<SitefinityProfile>(user);
                                return profile.Nickname;
                            }
                        }
                        return null;
                    });

                    dtoType.Field("UserEntity", new StringGraphType(), "The entity owner of the item.", resolve: (IResolveFieldContext context) =>
                    {
                        DynamicContent dcItem = context.Source as DynamicContent;
                        if (dcItem != null)
                        {
                            UserManager userManager = new UserManager();
                            UserProfileManager userProfileManager = new UserProfileManager();
                            var user = userManager.GetUser(dcItem.Owner);
                            var profile = userProfileManager.GetUserProfile<SitefinityProfile>(user);
                            if (profile != null)
                            {
                                return profile.DoesFieldExist("Entity") ? profile.GetValue("Entity")?.ToString() : string.Empty;
                            }
                        }
                        return null;
                    });


                    InputObjectGraphType subOptions = GenerateSubscriptionOptions(lowCaseClassName, dtoType, sitefinityType);
                    schema.RegisterType(subOptions);
                    schema.RegisterType(dtoType);
                    schema.RegisterType(responseType);
                    schema.RegisterType(saveType);
                });
        }

        private InputObjectGraphType GenerateSubscriptionOptions(string lowCaseClassName, ObjectGraphType dtoType, MetaTypeModel sitefinityType)
        {
            InputObjectGraphType subOptions = new InputObjectGraphType()
            {
                Name = "SubscribeOptions",
                Description = "Provides additional options for subscriptions on the type."
            };

            subOptions.AddField(new FieldType()
            {
                Name = "excludeCreate",
                ResolvedType = new BooleanGraphType(),
                Description = "True to subscribe to create events."
            });

            subOptions.AddField(new FieldType()
            {
                Name = "excludeDelete",
                ResolvedType = new BooleanGraphType(),
                Description = "True to subscribe delete events."
            });

            subOptions.AddField(new FieldType()
            {
                Name = "excludeModify",
                ResolvedType = new BooleanGraphType(),
                Description = "True to subscribe for update events."
            });

            subOptions.AddField(new FieldType()
            {
                Name = "excludeWorkflowChanged",
                ResolvedType = new BooleanGraphType(),
                Description = "True to subscribe for workflow changes."
            });

            ObjectGraphType fieldType = new ObjectGraphType()
            {
                Name = lowCaseClassName + "Subscription",
                Description = "Contains information about type changes.",
            };

            fieldType.AddField(new FieldType()
            {
                Name = "MasterId",
                Description = "The master ID.",
                ResolvedType = new GuidGraphType()
            });

            fieldType.AddField(new FieldType()
            {
                Name = "LiveId",
                Description = "The live ID.",
                ResolvedType = new GuidGraphType()
            });

            fieldType.AddField(new FieldType()
            {
                Name = "Operation",
                Description = "The operation being performed.",
                ResolvedType = new StringGraphType()
            });

            var field = RootSubscriptionsType.AddField(new FieldType()
            {
                Name = lowCaseClassName + "Subscription",
                Description = $"Allows you to subscribe for {lowCaseClassName} changes.",
                ResolvedType = dtoType,
                Arguments = new QueryArguments(new QueryArguments()
                {
                    new QueryArgument(subOptions)
                    {
                        Name = "SubscribeOptions"
                    }
                }),
                StreamResolver = new SourceStreamResolver<object>(ctx => ObservableFactory.GetObservable(ctx, sitefinityType.FullTypeName))
            });

            return subOptions;
        }

        private void RegisterMutationFields(MetaTypeModel sitefinityType, string lowCaseClassName, ObjectGraphType dtoType, QueryArguments mutationQueryArgs)
        {
            RootMutationType.Field(
                $"{lowCaseClassName}Save",
                dtoType,
                $"Updates or creates a {lowCaseClassName}",
                mutationQueryArgs,
                resolve: (context) =>
                {
                    return SaveHandlers.HandleSave(context, sitefinityType.FullTypeName);
                });

            RootMutationType.Field(
                $"{lowCaseClassName}Delete",
                dtoType,
                $"Deletes a {lowCaseClassName}",
                new QueryArguments()
                {
                    new QueryArgument(new GuidGraphType())
                    {
                        Name = "id",
                        Description = "The id of the item",
                        DefaultValue = Guid.Empty,
                    }
                },
                resolve: (context) =>
                {
                    return DeleteHandler.HandleDelete(context, sitefinityType.FullTypeName);
                });
        }

        private QueryArguments GetQueryArgs(InputObjectGraphType filterType, InputObjectGraphType sort)
        {
            return new QueryArguments()
            {
                new QueryArgument(new IntGraphType()) { Name = "_skip"},
                new QueryArgument(new IntGraphType()) { Name = "_take"},
                new QueryArgument(new StringGraphType()) { Name = "_provider" },
                new QueryArgument(filterType) { Name = "_filter",},
                new QueryArgument(sort) { Name = "_sort"}
            };
        }

        private static void AddMediaContentFields(InputObjectGraphType saveType)
        {
            saveType.Field<StringGraphType>("Base64Content");
            saveType.Field<StringGraphType>("Title");
            saveType.Field<StringGraphType>("Alt");
        }

        private void AddDynamicFields(MetaTypeModel sitefinityType, InputObjectGraphType stringFilters, InputObjectGraphType relatedFilters, InputObjectGraphType dateTimeFilters, InputObjectGraphType intFilters,
            InputObjectGraphType decimalFilters, InputObjectGraphType floatFilters, ObjectGraphType dtoType, InputObjectGraphType saveType, InputObjectGraphType filters, InputObjectGraphType orders)
        {
            InputObjectGraphType logicalOperators = new InputObjectGraphType()
            {
                Name = "logicalOperators",
                Description = "Adds  the ability to add or and and logical operators."
            };
            var orType = new InputObjectGraphType()
            {
                Name = $"Or{sitefinityType.ClassName}Filters",
                Description = "Allows OR operations between items"
            };
            var andType = new InputObjectGraphType()
            {
                Name = $"and{sitefinityType.ClassName}",
                Description = "Allows AND operations between items"
            };
            filters.AddField(new FieldType()
            {
                Name = "_lowLevelQuery",
                Description = "Allows you to define CSharp queries and send them to OpenAccess",
                ResolvedType = new StringGraphType()
            });

            sitefinityType.Fields.ForEach(sitefinityField =>
            {
                IGraphType fieldGraphType = GetGraphTypeForSitefinityFieldType(sitefinityField);
                if (sitefinityField.MetaAttributes.Any(a => a.Name == "RelatedType"))
                {
                    fieldsToRelate.Add(sitefinityField);
                    filters.AddField(new FieldType() { ResolvedType = relatedFilters, Name = sitefinityField.FieldName, });
                }
                else if (fieldGraphType != null)
                {
                    dtoType.Field(sitefinityField.FieldName, fieldGraphType, sitefinityField.Description, null);
                    if (fieldGraphType is StringGraphType)
                    {
                        filters.AddField(new FieldType() { ResolvedType = stringFilters, Name = sitefinityField.FieldName, });
                        orType.AddField(new FieldType() { ResolvedType = stringFilters, Name = sitefinityField.FieldName, });
                        andType.AddField(new FieldType() { ResolvedType = stringFilters, Name = sitefinityField.FieldName, });
                    }
                    else if (fieldGraphType is BooleanGraphType)
                    {
                        filters.AddField(new FieldType() { ResolvedType = new BooleanGraphType(), Name = sitefinityField.FieldName, });
                        orType.AddField(new FieldType() { ResolvedType = new BooleanGraphType(), Name = sitefinityField.FieldName, });
                        andType.AddField(new FieldType() { ResolvedType = new BooleanGraphType(), Name = sitefinityField.FieldName, });
                    }
                    else if (fieldGraphType is DateTimeGraphType)
                    {
                        filters.AddField(new FieldType() { ResolvedType = dateTimeFilters, Name = sitefinityField.FieldName, });
                        andType.AddField(new FieldType() { ResolvedType = dateTimeFilters, Name = sitefinityField.FieldName, });
                        orType.AddField(new FieldType() { ResolvedType = dateTimeFilters, Name = sitefinityField.FieldName, });
                    }
                    else if (fieldGraphType is IntGraphType)
                    {
                        filters.AddField(new FieldType() { ResolvedType = intFilters, Name = sitefinityField.FieldName });
                        andType.AddField(new FieldType() { ResolvedType = intFilters, Name = sitefinityField.FieldName, });
                        orType.AddField(new FieldType() { ResolvedType = intFilters, Name = sitefinityField.FieldName, });
                    }
                    else if (fieldGraphType is DecimalGraphType)
                    {
                        filters.AddField(new FieldType() { ResolvedType = decimalFilters, Name = sitefinityField.FieldName, });
                        andType.AddField(new FieldType() { ResolvedType = decimalFilters, Name = sitefinityField.FieldName, });
                        orType.AddField(new FieldType() { ResolvedType = decimalFilters, Name = sitefinityField.FieldName, });
                    }
                    else if (fieldGraphType is FloatGraphType)
                    {
                        filters.AddField(new FieldType() { ResolvedType = floatFilters, Name = sitefinityField.FieldName, });
                        andType.AddField(new FieldType() { ResolvedType = floatFilters, Name = sitefinityField.FieldName, });
                        orType.AddField(new FieldType() { ResolvedType = floatFilters, Name = sitefinityField.FieldName, });
                    }
                    orders.AddField(new FieldType() { ResolvedType = new StringGraphType(), Name = sitefinityField.FieldName, });
                    saveType.AddField(new FieldType() { Name = sitefinityField.FieldName, ResolvedType = fieldGraphType });
                }
            });
            if (orType.Fields.Count > 0)
            {
                filters.AddField(new FieldType() { ResolvedType = new ListGraphType(orType), Name = "or" });
            }
            if (andType.Fields.Count > 0)
            {
                filters.AddField(new FieldType() { ResolvedType = new ListGraphType(andType), Name = "and" });
            }
        }

        private IGraphType GetGraphTypeForSitefinityFieldType(MetaFieldModel sitefinityField)
        {
            string originalType = "";
            if (!string.IsNullOrWhiteSpace(sitefinityField.ClrType))
            {
                if (sitefinityField.MetaAttributes.Any(a => a.Name == "RelatedType"))
                {
                    originalType = sitefinityField.MetaAttributes.FirstOrDefault(a => a.Name == "RelatedType").Value;
                }
                return GetGraphTypeForClrType(sitefinityField.ClrType, originalType);
            }
            return null;
        }

        private IGraphType GetGraphTypeForClrType(string typeFullName, string originalType = "")
        {
            if (typeFullName == typeof(Lstring).FullName ||
                typeFullName == typeof(System.String).FullName ||
                typeFullName == typeof(ChoiceOption).FullName)
            {
                return new StringGraphType();
            }
            else if (typeFullName == typeof(int).FullName ||
                     typeFullName == typeof(int?).FullName)
            {
                return new IntGraphType();
            }
            else if (typeFullName == typeof(decimal).FullName ||
                     typeFullName == typeof(decimal?).FullName)
            {
                return new DecimalGraphType();
            }
            else if (typeFullName == typeof(float).FullName ||
                     typeFullName == typeof(float?).FullName)
            {
                return new FloatGraphType();
            }
            else if (typeFullName == typeof(DateTime).FullName ||
                     typeFullName == typeof(DateTime?).FullName)
            {
                return new DateTimeGraphType();
            }
            else if (typeFullName == typeof(bool).FullName)
            {
                return new BooleanGraphType();
            }

            return null;
        }
    }
}