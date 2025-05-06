using IL.AttributeBasedDI.Models;
using IL.AttributeBasedDI.Visualizer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IL.AttributeBasedDI.Visualizer.Extensions;

public static class DiRegistrationSummaryExtensions
{
    public static void AddVisualizer(this DiRegistrationSummary summary)
    {
        summary.Services.AddKeyedSingleton(Constants.ServiceGraphKey, summary.ServiceGraph);
        summary.Services.AddKeyedSingleton<IViewRenderService, ViewRenderService>(Constants.ViewRendererServiceKey);
    }
}