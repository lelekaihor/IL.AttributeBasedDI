namespace IL.AttributeBasedDI.Visualizer.Services;

public interface IViewRenderService
{
    Task<string> RenderViewToStringAsync(string viewPath, object model);
}