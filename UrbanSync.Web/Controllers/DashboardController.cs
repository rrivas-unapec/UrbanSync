using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UrbanSync.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        if (User.IsInRole("Administrador"))
            return View("Administrador");

        if (User.IsInRole("Supervisor") || User.IsInRole("SupervisorOperaciones"))
            return View("Supervisor");

        if (User.IsInRole("Tecnico") || User.IsInRole("AnalistaTecnico"))
            return View("Tecnico");

        return View("Ciudadano");
    }
}
