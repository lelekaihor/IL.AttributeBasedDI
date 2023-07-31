# IL.AttributeBasedDI
Control dependencies via custom Service Attribute - extends Microsoft.Extensions.DependencyInjection

# How to use

* Simply reference IL.AttributeBasedDI in your project
* Use service collection extension coming with library to activate functionality: `services.AddServiceAttributeBasedDependencyInjection()`
    * Allows to filter assemblies reflection search with optional parameter `assemblyFilters` ("MyProject.*" for example)
* Start decorating your classes with `[Service]` attribute with optional params: 
    * Lifetime - to define current service registration lifetime
    *  ServiceType - allows to specify main interface class is registered for, if null - will default to first interface on the list of iterfaces implemented or to itself.