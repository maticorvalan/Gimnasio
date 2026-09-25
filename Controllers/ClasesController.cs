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
    public class ClasesController : Controller
    {
        private readonly IClaseService claseService;
        private readonly IProfesorService profesorService;
        private readonly IUsuarioService usuarioService;
        private readonly ISuscripcionService suscripcionService;

        public ClasesController(IClaseService claseService, IProfesorService profesorService,
         IUsuarioService usuarioService, ISuscripcionService suscripcionService)
        {
            this.claseService = claseService;
            this.profesorService = profesorService;
            this.usuarioService = usuarioService;
            this.suscripcionService = suscripcionService;
        }
        private async Task CargarViewBags()
        {
            ViewBag.Profesores = new SelectList(
                await profesorService.ObtenerTodos(), "id", "nombre");
        }

        // GET: Clases
        public async Task<IActionResult> Index()
        {
            return View(await claseService.ObtenerTodos());
        }

        // GET: Clases/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clase = await claseService.ObtenerPorId(id.Value);
            if (clase == null)
            {
                return NotFound();
            }

            return View(clase);
        }

        // GET: Clases/Create
        public async Task<IActionResult> Create()
        {
            await CargarViewBags();
            return View();
        }

        // POST: Clases/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,nombre,descripcion,horario,dias_semana,hora_inicio,hora_fin,idprofesor,capacidad")] Clase clase)
        {
            
            if (ModelState.IsValid)
            {
                await claseService.Crear(clase);

                return RedirectToAction(nameof(Index));
            }
            return View(clase);
        }

        // GET: Clases/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clase = await claseService.ObtenerPorId(id.Value);
            if (clase == null)
            {
                return NotFound();
            }
            return View(clase);
        }

        // POST: Clases/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,nombre,descripcion,horario,dias_semana,hora_inicio,hora_fin,idprofesor,capacidad")] Clase clase)
        {
            if (id != clase.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await claseService.Actualizar(clase);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ClaseExists(clase.id))
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
            return View(clase);
        }

        // GET: Clases/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clase = await claseService.ObtenerPorId(id.Value);
            if (clase == null)
            {
                return NotFound();
            }

            return View(clase);
        }

        // POST: Clases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var clase = await claseService.ObtenerPorId(id);
            if (clase != null)
            {
                await claseService.Eliminar(clase.id);
            }

            return RedirectToAction(nameof(Index));
        }
        public class MarcarAsistenciaDto
        {
            public int idClase { get; set; }
            public string Dni { get; set; } = string.Empty;
        }
        [HttpPost]
        public async Task<IActionResult> MarcarAsistencia([FromBody] MarcarAsistenciaDto dto)
        {
            var resultado = await claseService.MarcarAsistencia(dto.idClase, dto.Dni);

            if (!resultado.Exito)
            {
                return Ok(new { exito = false, mensaje = resultado.Mensaje });
            }

            var usuario = await usuarioService.ObtenerPorDni(dto.Dni);
            if (usuario == null)
            {
                return Ok(new { exito = true, mensaje = "Check-in registrado con éxito." });
            }

            Suscripcion? suscripcion = null;
            try
            {
                suscripcion = await suscripcionService.ObtenerSuscripcionActiva(usuario.id);
            }
            catch { }

            var mensaje = $"¡Bienvenido, {usuario.nombre}!\n" +
                        $"Plan: {suscripcion?.Plan?.nombre ?? "Sin plan"}\n" +
                        $"Clases restantes: {(suscripcion?.clases_restantes?.ToString() ?? "Pase libre")}\n" +
                        $"Fecha de vencimiento: {(suscripcion?.fecha_fin.ToString("dd/MM/yyyy") ?? "N/A")}";


            return Ok(new
            {
                exito = true,
                mensaje = mensaje
            });
        }

        private async Task<bool> ClaseExists(int id)
        {
            return await claseService.ObtenerPorId(id) != null;
        }
    }
}
