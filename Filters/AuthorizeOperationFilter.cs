using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API_PortalSantosTech.Filters;

public class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAllowAnonymous = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                                    .OfType<AllowAnonymousAttribute>()
                                    .Any() == true
                                || context.MethodInfo.GetCustomAttributes(true)
                                    .OfType<AllowAnonymousAttribute>()
                                    .Any();

        if (hasAllowAnonymous)
            return;

        var hasAuthorize = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                               .OfType<AuthorizeAttribute>()
                               .Any() == true
                           || context.MethodInfo.GetCustomAttributes(true)
                               .OfType<AuthorizeAttribute>()
                               .Any();

        if (!hasAuthorize)
            return;

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                }
            ] = Array.Empty<string>()
        });

        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Não autenticado" });
        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Sem permissão" });
    }
}
