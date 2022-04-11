using Microsoft.OpenApi.Models;
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
                    Type = "object",
                    Properties =
                    {
                        ["gmlFile"] = new OpenApiSchema
                        {
                            Type = "file",
                            Format = "binary"
                        },
                        ["validate"] = new OpenApiSchema
                        {
                            Type = "boolean"
                        }
                    },
                    Required = new HashSet<string>() { "gmlFile" }
                }
            };
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = { ["multipart/form-data"] = mediaType }
            };
        }
    }
}
