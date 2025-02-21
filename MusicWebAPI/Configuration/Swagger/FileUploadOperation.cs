using Common.Utilities;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Configuration.Swagger
{
    public class FileUploadOperation : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileParameters = context.ApiDescription.ActionDescriptor.Parameters
                .Where(x => x.ParameterType == typeof(Microsoft.AspNetCore.Http.IFormFile))
                .ToList();

            if (fileParameters.Count > 0)
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content =
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["file"] = new OpenApiSchema
                                {
                                    Description = "Select file",
                                    Type = "file",
                                }
                            },
                            Required = new HashSet<string> { "file" }
                        }
                    }
                }
                };

                var fileParameterNames = fileParameters.Select(fp => fp.Name);

                operation.Parameters = operation.Parameters
                    .Where(p => !fileParameterNames.Contains(p.Name))
                    .ToList();
            }
        }
    }
}
