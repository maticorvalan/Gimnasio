using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Gimnasio.Data;
using Gimnasio.Models;
using Gimnasio.Services;
using Microsoft.AspNetCore.Authorization;

namespace Gimnasio.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class InscripcionesController : Controller
    {
        private readonly IInscripcionService inscripcionService;
        private readonly IUsuarioService usuarioService;
        private readonly IClaseService claseService;

        public InscripcionesController(IInscripcionService inscripcionService, IUsuarioService usuarioService, IClaseService claseService)
        {
            this.inscripcionService = inscripcionService;
            this.usuarioService = usuarioService;
            this.claseService = claseService;
        }

        private async Task CargarViewBag()
        {
            var usuarios = await usuarioService.ObtenerTodos();
            var clases = await claseService.ObtenerTodos();

            ViewBag.Usuarios = new SelectList(usuarios, "id", "nombre");
            ViewBag.Clases = new SelectList(clases, "id", "nombre");
        }

        // GET: Inscripciones
        public async Task<IActionResult> Index(int? pagina = 1, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            int pageNumber = pagina ?? 1;
            int pageSize = 5;

            var (inscripciones, total) = await inscripcionService.ObtenerPaginado(
                pageNumber, pageSize, fechaInicio, fechaFin);

            int totalPaginas = (int)Math.Ceiling((double)total / pageSize);

            ViewBag.Pagina = pageNumber;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");

            return View(inscripciones);
        }

        [HttpGet]
        public async Task<IActionResult> Ingresos(DateTime? fecha)
        {
            var fechaSeleccionada = fecha ?? DateTime.Today;
            var ingresos = await inscripcionService.ObtenerIngresosPorFecha(fechaSeleccionada, fechaSeleccionada);

            ViewBag.FechaSeleccionada = fechaSeleccionada;
            ViewBag.TotalIngresos = ingresos.Count;

            return View(ingresos);
        }

        // GET: Inscripciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inscripcion = await inscripcionService.ObtenerPorId(id.Value);
            if (inscripcion == null)
            {
                return NotFound();
            }

            return View(inscripcion);
        }

        // GET: Inscripciones/Create
        public async Task<IActionResult> Create()
        {
            await CargarViewBag();
            return View();
        }

        // POST: Inscripciones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,idusuario,idclase,fecha")] Inscripcion inscripcion)
        {
            if (ModelState.IsValid)
            {
                var creado = await inscripcionService.Crear(inscripcion);
                if (!creado.Exito){
                    ModelState.AddModelError(string.Empty, creado.Mensaje);
                    return View(inscripcion);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(inscripcion);
        }

        // GET: Inscripciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inscripcion = await inscripcionService.ObtenerPorId(id.Value);
            if (inscripcion == null)
            {
                return NotFound();
            }
            return View(inscripcion);
        }

        // POST: Inscripciones/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,idusuario,idclase,fecha")] Inscripcion inscripcion)
        {
            if (id != inscripcion.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var actualizado = await inscripcionService.Actualizar(inscripcion);
                    if (!actualizado.Exito)
                    {
                        ModelState.AddModelError(string.Empty, actualizado.Mensaje);
                        return View(inscripcion);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await InscripcionExists(inscripcion.id))
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
            return View(inscripcion);
        }

        // GET: Inscripciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inscripcion = await inscripcionService.ObtenerPorId(id.Value);
            if (inscripcion == null)
            {
                return NotFound();
            }

            return View(inscripcion);
        }

        // POST: Inscripciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inscripcion = await inscripcionService.ObtenerPorId(id);
            if (inscripcion == null)
            {
                return NotFound();
            }

            var eliminado = await inscripcionService.Eliminar(id);
            if (!eliminado.Exito)
            {
                TempData["Error"] = eliminado.Mensaje;
            }
            

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> InscripcionExists(int id)
        {
            return await inscripcionService.ObtenerPorId(id) != null;
        }

        public async Task<IActionResult> CheckIn(int id)
        {
            var inscripcion = await inscripcionService.ObtenerPorId(id);
            if (inscripcion == null)
            {
                return NotFound();
            }

            var checkIn = await inscripcionService.MarcarAsistencia(id, true);
            if (!checkIn.Exito)
            {
                TempData["Error"] = checkIn.Mensaje;
            } else
            {
                TempData["Exito"] = "Asistencia marcada correctamente.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
