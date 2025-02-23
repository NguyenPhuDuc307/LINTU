using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers.Components;

public class FooterViewComponent : ViewComponent
{
    public Task<IViewComponentResult> InvokeAsync()
    {
        return Task.FromResult((IViewComponentResult)View("Default"));
    }
}