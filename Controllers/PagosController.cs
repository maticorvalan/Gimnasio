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
    public class PagosController : Controller
    {
        private readonly IPagoService pagoService;
        private readonly IUsuarioService _usuarioService;
        private readonly ISuscripcionService _suscripcionService;


        public PagosController(IPagoService pagoService, IUsuarioService usuarioService, ISuscripcionService suscripcionService)
        {
            this.pagoService = pagoService;
            _usuarioService = usuarioService;
            _suscripcionService = suscripcionService;
        }
        // GET: Pagos
        public async Task<IActionResult> Index(int pagina = 1, int pageSize = 5, DateTime? fecha = null)
        {
            if (pagina < 1) pagina = 1;
            if (pageSize < 1) pageSize = 5;
            

            var resultado = await pagoService.ObtenerPaginado(pagina, pageSize, fecha);
            var hoy = DateTime.Today;
            var (totalMes, cantidadPagosMes) = await pagoService.ObtenerResumenMensual(hoy.Year, hoy.Month);

            ViewBag.RecaudacionMes = totalMes;
            ViewBag.CantidadPagosMes = cantidadPagosMes;
            ViewBag.PaginaActual = pagina;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRegistros = resultado.Total;
            ViewBag.TotalPaginas = Math.Max(1, (int)Math.Ceiling(resultado.Total / (double)pageSize));
            ViewBag.FechaFiltro = fecha?.ToString("yyyy-MM-dd") ?? string.Empty;

            return View(resultado.Items);
        }

        // GET: Pagos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pago = await pagoService.ObtenerPorId(id.Value);
            if (pago == null)
            {
                return NotFound();
            }

            return View(pago);
        }

        // GET: Pagos/Create
        public async Task<IActionResult> Create()
        {
            await CargarViewBags();
            return View();
        }
        // POST: Pagos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,idusuario,idsuscripcion,monto,fecha_pago,metodo_pago")] Pago pago)
        {
            if (ModelState.IsValid)
            {
                var resultado = await pagoService.CrearPago(pago);
                if (!resultado.Exito)
                {
                    ModelState.AddModelError("", resultado.Mensaje);
                    return View(pago);
                }
                return RedirectToAction(nameof(Index));
      
                  }
            return View(pago);
        }
        // GET: Pagos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pago = await pagoService.ObtenerPorId(id.Value);
            if (pago == null)
            {
                return NotFound();
            }
            var suscripciones = await _suscripcionService.ObtenerSuscripcionesActivasPorUsuario(pago.idusuario);
            ViewBag.Suscripciones = new SelectList(
                suscripciones.Select(s => new {
                    id = s.id,
                    texto = $"{s.Plan?.nombre} — vence {s.fecha_fin:dd/MM/yyyy}"
                }),
                "id", "texto",
                pago.idsuscripcion
            );
            return View(pago);
        }
        // POST: Pagos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,idusuario,idsuscripcion,monto,fecha_pago,metodo_pago")] Pago pago)
        {
            if (id != pago.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
               try
                {
                    var actualizado = await pagoService.ActualizarPago(pago);
                    if (!actualizado.Exito)
                    {
                        ModelState.AddModelError(string.Empty, actualizado.Mensaje);
                        return View(pago);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await PagoExists(pago.id))
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
            return View(pago);
        }
        // GET: Pagos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var pago = await pagoService.ObtenerPorId(id.Value);
            if (pago == null)
            {
                return NotFound();
            }
            return View(pago);
        }
        // POST: Pagos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pago = await pagoService.ObtenerPorId(id);
            if (pago != null)
            {
                var resultado = await pagoService.EliminarPago(id);
                if (!resultado.Exito)
                {
                    TempData["ErrorMessage"] = resultado.Mensaje;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        private async Task<bool> PagoExists(int id)
        {
            return await pagoService.ObtenerPorId(id) != null;
        }
        private async Task CargarViewBags()
        {
            ViewBag.Usuarios = new SelectList(
                await _usuarioService.ObtenerTodos(), "id", "nombre");

            // Mostrás algo útil en el select, por ejemplo el nombre del usuario + fechas
            var suscripciones = await _suscripcionService.ObtenerTodos();
            ViewBag.Suscripciones = new SelectList(
                suscripciones.Select(s => new {
                    id = s.id,
                    texto = $"{s.Usuario?.nombre} — {s.fecha_inicio:dd/MM/yy} al {s.fecha_fin:dd/MM/yy}"
                }),
                "id", "texto"
            );
        }

        [HttpGet("/api/suscripciones-por-usuario/{idUsuario}")]
        public async Task<IActionResult> GetSuscripcionesPorUsuario(int idUsuario)
        {
            var suscripciones = await _suscripcionService
                .ObtenerSuscripcionesActivasPorUsuario(idUsuario);

            var resultado = suscripciones.Select(s => new {
                id    = s.id,
                texto = $"{s.Plan?.nombre} — vence {s.fecha_fin:dd/MM/yyyy}",
                monto = s.Plan?.precio  // ← para autocompletar el monto
            });
            return Ok(resultado);
        }
        
    }
}