using IL.AttributeBasedDI.Models;
using IL.AttributeBasedDI.Visualizer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace IL.AttributeBasedDI.Visualizer.MinimalApi;

public static class VisualizerEndpointExtensions
{
    public static RouteHandlerBuilder MapDiVisualizerEndpoint(this WebApplication app, string visualizerPath = "diVisualizer")
    {
        return app.MapGet(visualizerPath.TrimStart('/'),
            async ([FromKeyedServices(Constants.ServiceGraphKey)] ServiceGraph serviceGraph, 
                [FromKeyedServices(Constants.ViewRendererServiceKey)] IViewRenderService viewRenderService) =>
            {
                var content = await viewRenderService.RenderViewToStringAsync(Constants.DiRegistrationSummaryViewPath, serviceGraph);
                return Results.Content(content, "text/html");
            });
    }
}