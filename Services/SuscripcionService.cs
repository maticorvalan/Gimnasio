    using Gimnasio.Models;
    using Gimnasio.Models.DTOs;
    using Microsoft.EntityFrameworkCore;
    using Gimnasio.Data;

    namespace Gimnasio.Services
    {
        public class SuscripcionService : ISuscripcionService
        {
            private readonly ApplicationDbContext _context;

            public SuscripcionService(ApplicationDbContext context)
            {
                _context = context;
            }

            private IQueryable<Suscripcion> QueryBase()
            {
                return _context.Suscripciones
                    .Include(s => s.Usuario)
                    .Include(s => s.Plan);
            }
            public async Task<List<Suscripcion>> ObtenerTodos()
            {
                return await QueryBase()
                    .ToListAsync()
                    ;
            }

            public async Task<Suscripcion> ObtenerPorId(int id)
            {
                var suscripcion = await QueryBase()
                    .FirstOrDefaultAsync(s => s.id == id);
                
                if (suscripcion == null)
                    throw new KeyNotFoundException($"No se encontró una suscripción con ID {id}");
                
                return suscripcion;
            }

            public async Task<ResultadoOperacion> Crear(CrearSuscripcionDto dto)
            {
                // Validar que el usuario existe
                var usuario = await _context.Usuarios.FindAsync(dto.idusuario);
                if (usuario == null)
                    return ResultadoOperacion.Fallo("El usuario no existe.");

                var plan = await _context.Planes.FindAsync(dto.idplan);
                if (plan == null)
                    return ResultadoOperacion.Fallo("El plan no existe.");

                var suscripcion = new Suscripcion
                {
                    idusuario        = dto.idusuario,
                    idplan           = dto.idplan,
                    fecha_inicio     = DateTime.Now,
                    fecha_fin        = DateTime.Now.AddMonths(1),
                    clases_restantes = plan.limite_clases,
                    activa           = true
                };

                _context.Suscripciones.Add(suscripcion);
                await _context.SaveChangesAsync();

                var pago = new Pago
                {
                    idusuario     = dto.idusuario,
                    idsuscripcion = suscripcion.id,
                    monto         = plan.precio,
                    fecha_pago    = DateTime.Now,
                    metodo_pago   = dto.metodo_pago
                };

                _context.Pagos.Add(pago);
                var guardado = await _context.SaveChangesAsync() > 0;

                return guardado
                    ? ResultadoOperacion.Ok()
                    : ResultadoOperacion.Fallo("Error al registrar el pago.");

            }

            public async Task<ResultadoOperacion> Actualizar(Suscripcion suscripcion)
            {
                var suscripcionExistente = await _context.Suscripciones.FindAsync(suscripcion.id);
                if (suscripcionExistente == null)
                    return ResultadoOperacion.Fallo($"No se encontró una suscripción con ID {suscripcion.id}");

                suscripcionExistente.idusuario = suscripcion.idusuario;
                suscripcionExistente.idplan = suscripcion.idplan;
                suscripcionExistente.fecha_inicio = suscripcion.fecha_inicio;
                suscripcionExistente.fecha_fin = suscripcion.fecha_fin;
                suscripcionExistente.clases_restantes = suscripcion.clases_restantes;
                suscripcionExistente.activa = suscripcion.activa;

                _context.Suscripciones.Update(suscripcionExistente);
                var guardado = await _context.SaveChangesAsync() > 0;
                return guardado
                    ? ResultadoOperacion.Ok()
                    : ResultadoOperacion.Fallo("Error al actualizar la suscripción.");
            }

            public async Task<ResultadoOperacion> Eliminar(int id)
            {
                var suscripcion = await _context.Suscripciones.FindAsync(id);
                if (suscripcion == null)
                    return ResultadoOperacion.Fallo($"No se encontró una suscripción con ID {id}");

                _context.Suscripciones.Remove(suscripcion);
                var guardado = await _context.SaveChangesAsync() > 0;
                return guardado
                    ? ResultadoOperacion.Ok()
                    : ResultadoOperacion.Fallo("Error al eliminar la suscripción.");
            }

            public async Task<int> ObtenerCantidad()
            {
                return await _context.Suscripciones.CountAsync();
            }

            public async Task<ResultadoOperacion> DescontarClase(int idSuscripcion)
            {
                var suscripcion = await _context.Suscripciones.FindAsync(idSuscripcion);
                if (suscripcion == null)
                    return ResultadoOperacion.Fallo($"No se encontró una suscripción con ID {idSuscripcion}");

                if (suscripcion.clases_restantes == null)
                    return ResultadoOperacion.Ok(); // No hay nada que descontar en pase libre

                if (suscripcion.clases_restantes <= 0)
                    return ResultadoOperacion.Fallo("No quedan clases disponibles.");

                suscripcion.clases_restantes--;

                _context.Suscripciones.Update(suscripcion);
                var guardado = await _context.SaveChangesAsync() > 0;
                return guardado
                    ? ResultadoOperacion.Ok()
                    : ResultadoOperacion.Fallo("Error al descontar la clase.");
            }

            public async Task<List<Suscripcion>> ObtenerSuscripcionesActivasPorUsuario(int idUsuario)
            {
                
                return await QueryBase()
                    .Where(s => s.idusuario == idUsuario && s.activa && s.fecha_fin > DateTime.Now)
                    .ToListAsync();
            }

            public async Task<Suscripcion> ObtenerSuscripcionActiva(int idUsuario)
            {
                return await QueryBase()
                    .Where(s => s.idusuario == idUsuario && s.activa && s.fecha_fin > DateTime.Now)
                    .FirstOrDefaultAsync()
                    ?? throw new InvalidOperationException("Sin suscripción activa");
            }
            public async Task<List<Suscripcion>> ObtenerSuscripcionessPorFecha(DateTime fecha)
            {
                return await QueryBase()
                    .Where(i => i.fecha_inicio.Date == fecha.Date)
                    .OrderByDescending(i => i.fecha_inicio)
                    .ToListAsync();
            }
           public async Task<(List<Suscripcion> Items, int Total)> ObtenerPaginado(
                int pagina, int pageSize, DateTime? fecha = null, 
                string? nombreUsuario = null, string? estado = null)
            {
                var query = QueryBase();

                if (fecha.HasValue)
                {
                    query = query.Where(i => i.fecha_inicio.Date == fecha.Value.Date);
                }

                if (!string.IsNullOrWhiteSpace(nombreUsuario))
                {
                    query = query.Where(s => s.Usuario != null && s.Usuario.nombre.Contains(nombreUsuario));
                }

                if (!string.IsNullOrWhiteSpace(estado))
                {
                    if (estado == "vigente")
                        query = query.Where(s => s.activa);
                    else if (estado == "caducada")
                        query = query.Where(s => !s.activa);
                }

                // Contamos el total ANTES de paginar (para calcular TotalPaginas)
                var total = await query.CountAsync();

                var items = await query
                    .OrderByDescending(i => i.fecha_inicio)
                    .Skip((pagina - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, total);
            }
        }
    }
