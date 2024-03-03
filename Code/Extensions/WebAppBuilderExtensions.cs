using Microsoft.AspNetCore.Builder;

namespace IL.AttributeBasedDI.Extensions;

public static class WebAppBuilderExtensions
{
    public static WebApplicationBuilder AddServiceAttributeBasedDependencyInjection(this WebApplicationBuilder builder)
    {
        builder.Services.AddServiceAttributeBasedDependencyInjection();
        return builder;
    }

    public static WebApplicationBuilder AddServiceAttributeBasedDependencyInjection(this WebApplicationBuilder builder, params string[] assemblyFilters)
    {
        builder.Services.AddServiceAttributeBasedDependencyInjection(assemblyFilters);
        return builder;
    }
}