using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Geonorge.Validator.Web
{
    public class MultipartOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.RelativePath != "MapDocument")
                return;

            var mediaType = new OpenApiMediaType()
            {
                Schema = new OpenApiSchema()
                {
                    Type = JsonSchemaType.Object,
                    Properties = new Dictionary<string, IOpenApiSchema>
                    {
                        ["gmlFile"] = new OpenApiSchema
                        {
                            Type = JsonSchemaType.String,
                            Format = "binary"
                        },
                        ["validate"] = new OpenApiSchema
                        {
                            Type = JsonSchemaType.Boolean
                        }
                    },
                    Required = new HashSet<string>() { "gmlFile" }
                }
            };
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType> { ["multipart/form-data"] = mediaType }
            };
        }
    }
}
