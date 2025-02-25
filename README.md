[![NuGet version (IL.AttributeBasedDI)](https://img.shields.io/nuget/v/IL.AttributeBasedDI.svg?style=flat-square)](https://www.nuget.org/packages/IL.AttributeBasedDI/)
# IL.AttributeBasedDI
Control dependencies and decorators via custom attributes - extends Microsoft.Extensions.DependencyInjection.

> **Note:** Starting from version **2.0.0**, only **.NET 8 or higher** is supported.

---

# How to Use

1. Reference `IL.AttributeBasedDI` in your project.
2. Use the registration extensions provided by the library to activate functionality:
   - `services.AddServiceAttributeBasedDependencyInjection()` for `IServiceCollection`.
   - `builder.AddServiceAttributeBasedDependencyInjection()` for `WebApplicationBuilder`.
3. Optionally, filter assemblies for reflection search using the `assemblyFilters` parameter (e.g., `"MyProject.*"`).

---

# Attributes

## `[Service]`
Use this attribute to automatically register classes in the DI container.

### Parameters:
- **Lifetime**: Defines the service registration lifetime (`Singleton`, `Scoped`, or `Transient`).
- **ServiceType**: Specifies the service type for DI registration. If `null`, the service type is automatically resolved:
  - From the first interface the class implements, or
  - The class itself if no interfaces are implemented.
- **Key** (`.NET 8+`): Specifies a key for keyed service registration.
- **Feature** (optional): Specifies a feature flag to conditionally register the service.

---

## `[Decorator]`
Use this attribute to automatically register decorators for specific services.

### Parameters:
- **ServiceType**: Specifies the service type to decorate. If `null`, the service type is automatically resolved from the first interface the class implements.
- **DecorationOrder**: Defines the order of decoration. Lower values are closer to the original implementation in the execution chain.
- **Key** (`.NET 8+`): Specifies a key for keyed decorator registration.
- **Feature** (optional): Specifies a feature flag to conditionally register the decorator.

---

# Examples

## Basic Usage

### IService resolves to:
- `DecoratorA`
  - Wrapping `SampleService`

```csharp
[Service]
class SampleService : IService {}

[Decorator]
class DecoratorA : IService {}
```
### IService resolves to:
- `DecoratorB`
    - `Wrapping DecoratorA`
        - `Wrapping SampleService`

```csharp
[Service(serviceType: typeof(IService), lifetime: Lifetime.Singleton)]
class SampleService : IService {}

[Decorator(serviceType: typeof(IService), decorationOrder: 1)]
class DecoratorA : IService 
{
    public DecoratorA(IService service)
    {
        // `service` here is actually `SampleService`
    }
}

[Decorator(serviceType: typeof(IService), decorationOrder: 2)]
class DecoratorB : IService 
{
    public DecoratorB(IService service)
    {
        // `service` here is actually `DecoratorA`
    }
}
```

## .NET 8 Keyed Services

```csharp
[Service(Key = "randomKey")]
class SampleServiceDefault : IService {}

[Service(Key = "testKey")]
class SampleService : IService {}

[Decorator(Key = "testKey")]
class DecoratorA : IService {}

public class Test
{
    public Test(
        [FromKeyedServices("randomKey")] IService randomSvc,
        [FromKeyedServices("testKey")] IService svc)
    {
        // `randomSvc` resolves to `SampleServiceDefault`
        // `svc` resolves to `DecoratorA` wrapping `SampleService`
    }
}
```

## Feature Flags
> **Note:** Starting from version 2.0.0, you can conditionally register services and decorators based on feature flags.

```csharp

[Flags]
public enum Features
{
    None = 0,
    FeatureA = 1 << 0,
    FeatureB = 1 << 1,
    FeatureC = 1 << 2
}

[Service<Features>(Feature = Features.FeatureA)]
class FeatureAService : IService {}

[Service<Features>(Feature = Features.FeatureB)]
class FeatureBService : IService {}

[Decorator<Features>(Feature = Features.FeatureA)]
class FeatureADecorator : IService {}

[Decorator<Features>(Feature = Features.FeatureB)]
class FeatureBDecorator : IService {}

```

```csharp

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceAttributeBasedDependencyInjection<Features>(options =>
{
    options.ActiveFeatures = Features.FeatureA | Features.FeatureB;
});

```
### or appsettings.json based:
```json
{
  "DIFeatureFlags": ["FeatureA", "FeatureB"]
}
```
### and then you can ignore options and use as:
```csharp

builder.AddServiceAttributeBasedDependencyInjection<Features>();

```

## Migration to Version 2.0.0

Starting from version 2.0.0, only .NET 8 or higher is supported. If you're upgrading from an earlier version, ensure your project targets .NET 8 or higher.