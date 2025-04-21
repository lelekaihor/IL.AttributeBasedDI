using IL.AttributeBasedDI.Options;
using Microsoft.AspNetCore.Builder;

namespace IL.AttributeBasedDI.Extensions;

public static class WebAppBuilderExtensions
{
    public static WebApplicationBuilder AddServiceAttributeBasedDependencyInjection(this WebApplicationBuilder builder)
    {
        builder.Services.AddServiceAttributeBasedDependencyInjection(builder.Configuration);
        return builder;
    }

    public static WebApplicationBuilder AddServiceAttributeBasedDependencyInjection(
        this WebApplicationBuilder builder,
        params string[] assemblyFilters)
    {
        builder.Services.AddServiceAttributeBasedDependencyInjection(builder.Configuration, assemblyFilters);
        return builder;
    }

    public static WebApplicationBuilder AddServiceAttributeBasedDependencyInjection(
        this WebApplicationBuilder builder,
        Action<FeatureBasedDIOptions> configureOptions)
    {
        builder.Services.AddServiceAttributeBasedDependencyInjection(builder.Configuration, configureOptions);
        return builder;
    }

    public static WebApplicationBuilder AddServiceAttributeBasedDependencyInjection(
        this WebApplicationBuilder builder,
        Action<FeatureBasedDIOptions> configureOptions,
        params string[] assemblyFilters)
    {
        builder.Services.AddServiceAttributeBasedDependencyInjection(builder.Configuration, configureOptions, assemblyFilters);
        return builder;
    }
}