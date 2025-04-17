using Microsoft.AspNetCore.Mvc;

namespace ASC.Web
{
    public class MVCarea : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
