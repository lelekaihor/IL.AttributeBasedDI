using Microsoft.Extensions.DependencyInjection;

namespace IL.AttributeBasedDI.Models;

public sealed class DiRegistrationSummary(IServiceCollection services)
{
    public IServiceCollection Services { get; init; } = services;
    public ServiceGraph ServiceGraph { get; init; } = new();
}