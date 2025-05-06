using IL.AttributeBasedDI.Models;
using IL.AttributeBasedDI.Options;
using Microsoft.AspNetCore.Builder;

namespace IL.AttributeBasedDI.Extensions;

public static class WebAppBuilderExtensions
{
    public static DiRegistrationSummary AddServiceAttributeBasedDependencyInjection(this WebApplicationBuilder builder)
    {
        return builder.Services.AddServiceAttributeBasedDependencyInjection(builder.Configuration);
    }

    public static DiRegistrationSummary AddServiceAttributeBasedDependencyInjection(
        this WebApplicationBuilder builder,
        params string[] assemblyFilters)
    {
        return builder.Services.AddServiceAttributeBasedDependencyInjection(builder.Configuration, assemblyFilters);
    }

    public static DiRegistrationSummary AddServiceAttributeBasedDependencyInjection(
        this WebApplicationBuilder builder,
        Action<FeatureBasedDIOptions> configureOptions)
    {
        return builder.Services.AddServiceAttributeBasedDependencyInjection(builder.Configuration, configureOptions);
    }

    public static DiRegistrationSummary AddServiceAttributeBasedDependencyInjection(
        this WebApplicationBuilder builder,
        Action<FeatureBasedDIOptions> configureOptions,
        params string[] assemblyFilters)
    {
        return builder.Services.AddServiceAttributeBasedDependencyInjection(builder.Configuration, configureOptions, assemblyFilters);
    }
}