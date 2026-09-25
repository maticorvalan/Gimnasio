using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gimnasio.Models;
using Gimnasio.Models.DTOs;
using Gimnasio.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Gimnasio.Controllers
{
    [Authorize(AuthenticationSchemes = 
    JwtBearerDefaults.AuthenticationScheme + "," + 
    CookieAuthenticationDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class SuscripcionesApiController : ControllerBase
    {
        private readonly ISuscripcionService suscripcionService;

        public SuscripcionesApiController(ISuscripcionService suscripcionService)
        {
            this.suscripcionService = suscripcionService;
        }

        // GET: api/SuscripcionesApi
        [HttpGet]
        public async Task<IActionResult> GetSuscripciones()
        {
            var lista = await suscripcionService.ObtenerTodos();
            return Ok(lista);
        }
        // GET: api/SuscripcionesApi/paginado?pagina=1&pageSize=5&fecha=2026-07-30&nombreUsuario=juan&estado=vigente
        [HttpGet("paginado")]
        public async Task<IActionResult> GetSuscripcionesPaginadas(
            [FromQuery] int pagina = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] DateTime? fecha = null,
            [FromQuery] string? nombreUsuario = null,
            [FromQuery] string? estado = null)
        {
            if (pagina < 1) pagina = 1;
            if (pageSize < 1) pageSize = 5;

            var resultado = await suscripcionService.ObtenerPaginado(pagina, pageSize, fecha, nombreUsuario, estado);

            return Ok(new
            {
                items = resultado.Items,
                total = resultado.Total,
                pagina,
                pageSize,
                totalPages = (int)Math.Ceiling(resultado.Total / (double)pageSize)
            });
        }

        // GET: api/SuscripcionesApi/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSuscripcion(int id)
        {
            var suscripcion = await suscripcionService.ObtenerPorId(id);
            if (suscripcion == null) return NotFound();

            return Ok(suscripcion);
        }

        // POST: api/SuscripcionesApi
        [HttpPost]
        public async Task<IActionResult> PostSuscripcion([FromBody] CrearSuscripcionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

                var resultado = await suscripcionService.Crear(dto);

                if (!resultado.Exito)
                    return BadRequest(resultado.Mensaje);

                return Ok(resultado);
        }

        // PUT: api/SuscripcionesApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSuscripcion(int id, [FromBody] Suscripcion suscripcion)
        {
            if (id != suscripcion.id) return BadRequest("El ID no coincide.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await suscripcionService.Actualizar(suscripcion);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await suscripcionService.ObtenerPorId(id) == null) return NotFound();
                throw;
            }

            return NoContent(); // 204 No Content es el estándar para respuestas PUT exitosas sin cuerpo
        }

        // DELETE: api/SuscripcionesApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSuscripcion(int id)
        {
            var suscripcion = await suscripcionService.ObtenerPorId(id);
            if (suscripcion == null) return NotFound();

            await suscripcionService.Eliminar(id);
            return Ok(new { message = "Suscripción eliminada con éxito." });
        }
    }
}