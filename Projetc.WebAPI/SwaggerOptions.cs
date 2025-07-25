using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Project.DTO.Common;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text;

namespace Projetc.WebAPI
{

    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        readonly IApiVersionDescriptionProvider provider;
        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => this.provider = provider;

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                try
                {
                    options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
                }
                catch (Exception e)
                {
                    Console.WriteLine(description); Console.WriteLine(e);
                }
            }
        }
        static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo()
            {
                Title = "BABER API",
                Version = description.GroupName,
                Description = "",
                Contact = new OpenApiContact()
                {
                    Name = "rickcarloz",
                    Email = "rickcarloz@dev.com.br",
                }
            };
            if (description.IsDeprecated)
            {
                info.Description += ". Essa não é a ultima Versão da API.";
            }
            return info;
        }
    }

    public class SwaggerDefaultValues : IOperationFilter
    {
        /// <summary>
        /// Applies the filter to the specified operation using the given context.
        /// </summary>
        /// <param name="operation">The operation to apply the filter to.</param>
        /// <param name="context">The current operation filter context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;
            if (operation.Parameters == null)
            {
                return;
            }
            foreach (var parameter in operation.Parameters)
            {
                var description = apiDescription.ParameterDescriptions.First(p => p.Name.ToUpper() == parameter.Name.ToUpper());
                if (parameter.Description == null)
                {
                    parameter.Description = description.ModelMetadata?.Description;
                }
                parameter.Required |= description.IsRequired;
            }
        }
    }


    public class MySwaggerSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema?.Properties == null)
            {
                return;
            }

            var ignoreDataMemberProperties = context.Type.GetProperties().Where(t => t.GetCustomAttribute<SwaggerExclude>() != null);
            foreach (var ignoreDataMemberProperty in ignoreDataMemberProperties)
            {
                var propertyToHide = schema.Properties.Keys.SingleOrDefault(x => x.ToLower() == ignoreDataMemberProperty.Name.ToLower());
                if (propertyToHide != null)
                {
                    schema.Properties.Remove(propertyToHide);
                }
            }
        }
    }

    public class SwaggerAddEnumDescriptions : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // add enum descriptions to result models
            foreach (var property in swaggerDoc.Components.Schemas.Where(x => x.Value?.Enum?.Count > 0))
            {
                IList<IOpenApiAny> propertyEnums = property.Value.Enum;
                if (propertyEnums != null && propertyEnums.Count > 0)
                {
                    property.Value.Description += DescribeEnum(propertyEnums, property.Key);
                }
            }

            // add enum descriptions to input parameters
            foreach (var pathItem in swaggerDoc.Paths.Values)
            {
                DescribeEnumParameters(pathItem.Operations, swaggerDoc);
            }
        }

        private void DescribeEnumParameters(IDictionary<OperationType, OpenApiOperation> operations, OpenApiDocument swaggerDoc)
        {
            if (operations != null)
            {
                foreach (var oper in operations)
                {
                    foreach (var param in oper.Value.Parameters)
                    {
                        var paramEnum = swaggerDoc.Components.Schemas.FirstOrDefault(x => x.Key == param.Name);
                        if (paramEnum.Value != null)
                        {
                            param.Description += DescribeEnum(paramEnum.Value.Enum, paramEnum.Key);
                        }
                    }
                }
            }
        }

        private Type GetEnumTypeByName(string enumTypeName)
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .FirstOrDefault(x => x.Name == enumTypeName);
        }

        private string DescribeEnum(IList<IOpenApiAny> enums, string proprtyTypeName)
        {
            List<string> enumDescriptions = new List<string>();
            var enumType = GetEnumTypeByName(proprtyTypeName);
            if (enumType == null)
                return null;

            foreach (IOpenApiAny enumOption in enums)
            {
                if (enumOption is OpenApiString @string)
                {
                    string enumString = @string.Value;

                    enumDescriptions.Add(string.Format("{0} = {1}", (int)Enum.Parse(enumType, enumString), enumString));
                }
                else if (enumOption is OpenApiInteger integer)
                {
                    int enumInt = integer.Value;

                    enumDescriptions.Add(string.Format("{0} = {1}", enumInt, Enum.GetName(enumType, enumInt)));
                }
            }

            return string.Join(", ", enumDescriptions.ToArray());
        }
    }

    public static class TypeExtensions
    {
        public static string FriendlyId(this Type type, bool fullyQualified = false)
        {
            var typeName = fullyQualified
                ? type.FullNameSansTypeArguments().Replace("+", ".")
                : type.Name;

            if (type.GetTypeInfo().IsGenericType)
            {
                var genericArgumentIds = type.GetGenericArguments()
                    .Select(t => t.FriendlyId(fullyQualified))
                    .ToArray();

                return new StringBuilder(typeName)
                    .Replace(string.Format("`{0}", genericArgumentIds.Count()), string.Empty)
                    .Append(string.Format("[{0}]", string.Join(",", genericArgumentIds).TrimEnd(',')))
                    .ToString();
            }

            return typeName;
        }

        internal static bool IsNullable(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        internal static bool IsFSharpOption(this Type type)
        {
            return type.FullNameSansTypeArguments() == "Microsoft.FSharp.Core.FSharpOption`1";
        }

        private static string FullNameSansTypeArguments(this Type type)
        {
            if (string.IsNullOrEmpty(type.FullName)) return string.Empty;

            var fullName = type.FullName;
            var chopIndex = fullName.IndexOf("[[");
            return (chopIndex == -1) ? fullName : fullName.Substring(0, chopIndex);
        }
    }

}
