using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gimnasio.Data;
using Gimnasio.Models;
using Gimnasio.Services;
using Microsoft.AspNetCore.Authorization;

namespace Gimnasio.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class PlanesController : Controller
    {
        private readonly IPlanService planService;

        public PlanesController(IPlanService planService)
        {
            this.planService = planService;
        }

        // GET: Planes
        public async Task<IActionResult> Index()
        {
            return View(await planService.ObtenerTodos());
        }

        // GET: Planes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plan = await planService.ObtenerPorId(id.Value);
            if (plan == null)
            {
                return NotFound();
            }

            return View(plan);
        }

        // GET: Planes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Planes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,nombre,descripcion,limite_clases,precio")] Plan plan)
        {
            if (ModelState.IsValid)
            {
                var creado = await planService.Crear(plan);
                if (!creado.Exito)
                {
                    ModelState.AddModelError(string.Empty, creado.Mensaje);
                    return View(plan);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(plan);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var plan = await planService.ObtenerPorId(id.Value);
            if(plan == null)
            {
                return NotFound();
            }
            return View(plan);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,nombre,descripcion,limite_clases,precio")] Plan plan)
        {
            if (id != plan.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var actualizado = await planService.Actualizar(plan);
                    if (!actualizado.Exito)
                    {
                        ModelState.AddModelError(string.Empty, actualizado.Mensaje);
                        return View(plan);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await PlanExists(plan.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(plan);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plan = await planService.ObtenerPorId(id.Value);
            if (plan == null)
            {
                return NotFound();
            }

            return View(plan);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var plan = await planService.ObtenerPorId(id);
            if (plan != null)
            {
                var eliminado = await planService.Eliminar(plan.id);
                if (!eliminado.Exito)
                {
                    TempData["Error"] = eliminado.Mensaje;
                }
            }

            return RedirectToAction(nameof(Index));
        }
        private async Task<bool> PlanExists(int id)
        {
            return await planService.ObtenerPorId(id) != null;
        }
        [HttpGet("api/[controller]")]
        public async Task<IActionResult> GetPlanes()
        {
            var lista = await planService.ObtenerTodos();
            return Ok(lista);
        }
    }
}