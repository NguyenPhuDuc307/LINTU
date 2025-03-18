using Microsoft.AspNetCore.Mvc;
using LMS.Data.Entities;

public class PaginationViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(Pagination model)
    {
        return View(model);
    }
}
