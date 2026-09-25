
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Gimnasio.Services;
using Gimnasio.Models;
using Microsoft.AspNetCore.Authorization;

namespace Gimnasio.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ProfesoresController : Controller
    {
        private readonly IProfesorService _profesorService;
        private readonly ImagenService _imagenService;

        public ProfesoresController(IProfesorService profesorService, ImagenService imagenService)
        {
            _profesorService = profesorService;
            _imagenService = imagenService;
        }

        // GET: Profesores
        public async Task<IActionResult> Index()
        {
            return View(await _profesorService.ObtenerTodos());
        }

        // GET: Profesores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profesor = await _profesorService.ObtenerPorId(id.Value);
            if (profesor == null)
            {
                return NotFound();
            }

            return View(profesor);
        }

        // GET: Profesores/Create
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,nombre,especialidad,rutaFoto")] Profesor profesor, IFormFile? avatar)
        {
            if (ModelState.IsValid)
            {
                var creado = await _profesorService.Crear(profesor);
                if (!creado.Exito)
                {
                    ModelState.AddModelError(string.Empty, creado.Mensaje);
                    return View(profesor);
                }
                if(avatar != null)
                {
                    var ruta = await _imagenService.GuardarImagen(avatar, "profesores", profesor.id);
                    if(!string.IsNullOrEmpty(ruta))
                    {
                        profesor.rutaFoto = ruta;
                        await _profesorService.Actualizar(profesor);
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(profesor);
        }

        // GET: Profesores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profesor = await _profesorService.ObtenerPorId(id.Value);
            if (profesor == null)
            {
                return NotFound();
            }
            return View(profesor);
        }

        // POST: Profesores/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,nombre,especialidad,rutaFoto")] Profesor profesor, IFormFile? avatar)
        {
            if (id != profesor.id)
            {
                return NotFound();
            }
            if(!ModelState.IsValid)
            {
                return View(profesor);
            }
            if (ModelState.IsValid)
            {
                try
                {

                    var profesorActual = await _profesorService.ObtenerPorId(profesor.id);
                    if (avatar != null && avatar.Length > 0)
{
                        // Le pasamos profesorActual.rutaFoto para que el servicio la elimine
                        var ruta = await _imagenService.GuardarImagen(avatar, "profesores", profesor.id, profesorActual.rutaFoto ?? string.Empty);
                        
                        if(!string.IsNullOrEmpty(ruta))
                        {
                            profesor.rutaFoto = ruta;
                        }
                    }
                    else
                    {
                        profesor.rutaFoto = profesorActual.rutaFoto;
                    }

                    var actualizado = await _profesorService.Actualizar(profesor);
                    if (!actualizado.Exito)
                    {
                        TempData["Error"] = actualizado.Mensaje;
                        ModelState.AddModelError(string.Empty, actualizado.Mensaje);
                        return View(profesor);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ProfesorExiste(profesor.id))
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
            return View(profesor);
        }

        // GET: Profesores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profesor = await _profesorService.ObtenerPorId(id.Value);
            if (profesor == null)
            {
                return NotFound();
            }

            return View(profesor);
        }

        // POST: Profesores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var profesor = await _profesorService.ObtenerPorId(id);
            if (profesor != null)
            {
                var eliminado = await _profesorService.Eliminar(id);
                if (!eliminado.Exito)
                {
                    TempData["Error"] = eliminado.Mensaje;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ProfesorExiste(int id)
        {
            return await _profesorService.ObtenerPorId(id) != null;
        }
    }
}
