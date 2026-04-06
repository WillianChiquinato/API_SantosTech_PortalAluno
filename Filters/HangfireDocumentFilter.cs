using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API_PortalSantosTech.Filters;

public class HangfireDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Paths["/hangfire"] = new OpenApiPathItem
        {
            Operations =
            {
                [OperationType.Get] = new OpenApiOperation
                {
                    Tags = new List<OpenApiTag>
                    {
                        new() { Name = "Hangfire" }
                    },
                    Summary = "Abrir painel do Hangfire",
                    Description = "Acessa o dashboard do Hangfire. Requer autenticação com usuário Admin.",
                    Responses = new OpenApiResponses
                    {
                        ["200"] = new OpenApiResponse
                        {
                            Description = "Dashboard carregado com sucesso."
                        },
                        ["401"] = new OpenApiResponse
                        {
                            Description = "Usuário não autenticado."
                        },
                        ["403"] = new OpenApiResponse
                        {
                            Description = "Usuário autenticado sem permissão de Admin."
                        }
                    }
                }
            }
        };
    }
}
