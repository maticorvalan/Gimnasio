using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Gimnasio.Controllers
{
    [Authorize(Roles = "Administrador")]
    // Este controlador solo sirve para cargar la vista
    public class SuscripcionesController : Controller
    {
        public IActionResult Index()
        {
            // Retorna Views/Suscripciones/Index.cshtml
            return View(); 
        }
    }
}