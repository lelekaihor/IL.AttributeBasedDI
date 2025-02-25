using IL.AttributeBasedDI.Models;
using IL.AttributeBasedDI.Options;
using Microsoft.AspNetCore.Builder;

namespace IL.AttributeBasedDI.Extensions;

public static class WebAppBuilderExtensions
    {
        // Non-generic version (no feature flags)
        public static WebApplicationBuilder AddServiceAttributeBasedDependencyInjection(this WebApplicationBuilder builder)
        {
            builder.Services.AddServiceAttributeBasedDependencyInjection<FeaturesNoop>(options => { });
            return builder;
        }

        // Non-generic version with assembly filters (no feature flags)
        public static WebApplicationBuilder AddServiceAttributeBasedDependencyInjection(
            this WebApplicationBuilder builder,
            params string[] assemblyFilters)
        {
            builder.Services.AddServiceAttributeBasedDependencyInjection<FeaturesNoop>(options => { }, assemblyFilters);
            return builder;
        }

        // Generic version with active features
        public static WebApplicationBuilder AddServiceAttributeBasedDependencyInjection<TFeatureFlag>(
            this WebApplicationBuilder builder,
            Action<FeatureBasedDIOptions<TFeatureFlag>> configureOptions)
            where TFeatureFlag : struct, Enum
        {
            builder.Services.AddServiceAttributeBasedDependencyInjection(configureOptions);
            return builder;
        }

        // Generic version with active features and assembly filters
        public static WebApplicationBuilder AddServiceAttributeBasedDependencyInjection<TFeatureFlag>(
            this WebApplicationBuilder builder,
            Action<FeatureBasedDIOptions<TFeatureFlag>> configureOptions,
            params string[] assemblyFilters)
            where TFeatureFlag : struct, Enum
        {
            builder.Services.AddServiceAttributeBasedDependencyInjection(configureOptions, assemblyFilters);
            return builder;
        }

        // Generic version with configuration and active features
        public static WebApplicationBuilder AddServiceAttributeBasedDependencyInjectionWithOptions<TFeatureFlag>(
            this WebApplicationBuilder builder,
            Action<FeatureBasedDIOptions<TFeatureFlag>> configureOptions)
            where TFeatureFlag : struct, Enum
        {
            builder.Services.AddServiceAttributeBasedDependencyInjectionWithOptions(builder.Configuration, configureOptions);
            return builder;
        }

        // Generic version with configuration, active features, and assembly filters
        public static WebApplicationBuilder AddServiceAttributeBasedDependencyInjectionWithOptions<TFeatureFlag>(
            this WebApplicationBuilder builder,
            Action<FeatureBasedDIOptions<TFeatureFlag>> configureOptions,
            params string[] assemblyFilters)
            where TFeatureFlag : struct, Enum
        {
            builder.Services.AddServiceAttributeBasedDependencyInjectionWithOptions(builder.Configuration, configureOptions, assemblyFilters);
            return builder;
        }
    }