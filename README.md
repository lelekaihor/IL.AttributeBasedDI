[![NuGet version (IL.AttributeBasedDI)](https://img.shields.io/nuget/v/IL.AttributeBasedDI.svg?style=flat-square)](https://www.nuget.org/packages/IL.AttributeBasedDI/)
# IL.AttributeBasedDI
Control dependencies and decorators via custom attributes - extends Microsoft.Extensions.DependencyInjection

# How to use

* Simply reference IL.AttributeBasedDI in your project
* Use registration extensions coming with library to activate functionality: `(IServiceCollection)services.AddServiceAttributeBasedDependencyInjection()` or `(WebApplicationBuilder)builder.AddServiceAttributeBasedDependencyInjection()`
    * Allows to filter assemblies reflection search with optional parameter `assemblyFilters` ("MyProject.*" for example)
* Use `[Service]` attribute for your classes with optional params for auto registration in DI container: 
    * Lifetime - to define current service registration lifetime
    * ServiceType - Specifies which service is target for DI registration. If left null/default service will be automatically retrieved either from first interface current class implements or the class itself will become a serviceType.
* Use `[Decorator]` attribute for your classes with optional params for auto registration of decorator for specific service type in DI container:
    * ServiceType - Specifies which service is target for decoration. If left null/default service will be automatically resolved to first interface current class implements.
    * DecorationLayer - Defines order of decoration. Lower decoration order will be closer to original implementation in chain of execution order. And, respectively, decorator with highest DecorationOrder will be executed last.

## Examples
IService resolves to:
* DecoratorA
    * wrapping a SampleService

```
[Service]
class SampleService : IService {}

[Decorator]
class DecoratorA : IService {}
```
IService resolves to:
* DecoratorB
    * wrapping a DecoratorA
        * wrapping a SampleService

```
[Service(serviceType: typeof(IService), lifetime: Lifetime.Singleton)]
class SampleService : IService {}

[Decorator(serviceType: typeof(IService), decorationLayer: 1)]
class DecoratorA : IService {}

[Decorator(serviceType: typeof(IService), decorationLayer: 2)]
class DecoratorB : IService {}
```