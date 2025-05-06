using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace IL.AttributeBasedDI.Visualizer.Services;

public sealed class ViewRenderService(IHttpContextAccessor httpContextAccessor,
    ICompositeViewEngine viewEngine, 
    ITempDataProvider tempDataProvider) : IViewRenderService
{
    public async Task<string> RenderViewToStringAsync(string viewPath, object model)
    {
        var actionContext = new ControllerContext
        {
            HttpContext = httpContextAccessor.HttpContext!,
            RouteData = new RouteData
            {
                Values =
                {
                    ["controller"] = "fake",
                    ["action"] = "fake"
                }
            },
            ActionDescriptor = new ControllerActionDescriptor
            {
                ActionName = "fake",
                ControllerName = "fake"
            }
        };

        var viewResult = viewEngine.GetView(string.Empty, viewPath, isMainPage: false);

        if (!viewResult.Success)
        {
            throw new InvalidOperationException($"View '{viewPath}' not found.");
        }

        var stringWriter = new StringWriter();
        var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model };
        var tempData = new TempDataDictionary(httpContextAccessor.HttpContext!, tempDataProvider);

        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            viewData,
            tempData,
            stringWriter,
            new HtmlHelperOptions()
        );

        await viewResult.View.RenderAsync(viewContext);
        return stringWriter.ToString();
    }
}