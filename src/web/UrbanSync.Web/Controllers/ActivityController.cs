using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UrbanSync.Web.Controllers;

[Authorize(Roles = "Administrador,Supervisor,SupervisorOperaciones")]
public class ActivityController : Controller
{
    public IActionResult Index()
    {
        ViewBag.Message = "La auditoria ahora pertenece a la API/BD UrbanSync. Agrega un endpoint de auditoria si quieres listar actividades aqui.";
        return View(new List<object>());
    }
}
