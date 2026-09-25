using Gimnasio.Models;
using Microsoft.EntityFrameworkCore;
using Gimnasio.Data;

namespace Gimnasio.Services
{
    public class InscripcionService : IInscripcionService
    {
        private readonly ApplicationDbContext _context;

        public InscripcionService(ApplicationDbContext context)
        {
            _context = context;
        }

        private IQueryable<Inscripcion> QueryBase()
        {
            return _context.Inscripciones
                .Include(i => i.Clase)
                .Include(i => i.Usuario);
        }

        public async Task<List<Inscripcion>> ObtenerTodos()
        {
            return await QueryBase()
                .ToListAsync();
        }

        public async Task<Inscripcion?> ObtenerPorId(int id)
        {
            return await QueryBase()
                .FirstOrDefaultAsync(i => i.id == id);
        }

        public async Task<ResultadoOperacion> Crear(Inscripcion inscripcion)
        {
            var cerrada = await EstaCerrada(inscripcion.idclase, inscripcion.fecha);
            if (cerrada)
                return ResultadoOperacion.Fallo("Los cupos para esta clase están cerrados hoy.");
                
            var clase = await _context.Clases
                .Include(c => c.Inscripciones)
                .FirstOrDefaultAsync(c => c.id == inscripcion.idclase);
            if (clase == null ){
                return ResultadoOperacion.Fallo("Clase no encontrada");
            }
            var inscriptosDelDia = await _context.Inscripciones
                    .CountAsync(i => i.idclase == inscripcion.idclase && i.fecha.Date == inscripcion.fecha.Date);
            if (inscriptosDelDia >= clase.capacidad){
                return ResultadoOperacion.Fallo("Capacidad máxima de la clase alcanzada");
            }
            var usuario = await _context.Usuarios.FindAsync(inscripcion.idusuario);
            if (usuario == null){
                return ResultadoOperacion.Fallo("Usuario no encontrado");
            }
            var yaInscripto = await _context.Inscripciones
                .AnyAsync(i => i.idusuario == inscripcion.idusuario 
                            && i.idclase == inscripcion.idclase 
                            && i.fecha.Date == inscripcion.fecha.Date);

            if (yaInscripto)
                return ResultadoOperacion.Fallo("Ya estás inscripto en esta clase ese día");
            _context.Inscripciones.Add(inscripcion);
            var guardado = await _context.SaveChangesAsync() > 0;
            return guardado 
                ? ResultadoOperacion.Ok() 
                : ResultadoOperacion.Fallo("Error al crear la inscripción.");
        }

        public async Task<ResultadoOperacion> Eliminar(int id)
        {
            var inscripcion = await _context.Inscripciones.FindAsync(id);
            if (inscripcion == null)
                return ResultadoOperacion.Fallo("Inscripción no encontrada");

            _context.Inscripciones.Remove(inscripcion);
            var guardado = await _context.SaveChangesAsync() > 0;
            return guardado 
                ? ResultadoOperacion.Ok() 
                : ResultadoOperacion.Fallo("Error al eliminar la inscripción.");
        }

        public async Task<ResultadoOperacion> Actualizar(Inscripcion inscripcion)
        {
            var inscripcionExistente = await _context.Inscripciones.FindAsync(inscripcion.id);
            if (inscripcionExistente == null)
                return ResultadoOperacion.Fallo("Inscripción no encontrada");

            inscripcionExistente.idclase = inscripcion.idclase;
            inscripcionExistente.idusuario = inscripcion.idusuario;
            inscripcionExistente.fecha = inscripcion.fecha;

            _context.Inscripciones.Update(inscripcionExistente);
            var guardado = await _context.SaveChangesAsync() > 0;
            return guardado 
                ? ResultadoOperacion.Ok() 
                : ResultadoOperacion.Fallo("Error al actualizar la inscripción.");
        }

        public async Task<int> ObtenerCantidad()
        {
            return await _context.Inscripciones.CountAsync();
        }

        public async Task<ResultadoOperacion> MarcarAsistencia(int idInscripcion, bool asistio)
        {
            var inscripcion = await _context.Inscripciones.FindAsync(idInscripcion);
            if (inscripcion == null)
                return ResultadoOperacion.Fallo("No se encontró la inscripción.");

            inscripcion.asistio = asistio;
            if (asistio)
            {
                inscripcion.fecha_checkin = DateTime.Now; // Registrar la hora de check-in
            }
            else
            {
                inscripcion.fecha_checkin = null; // Limpiar la hora de check-in si se marca como no asistió
            }

            _context.Inscripciones.Update(inscripcion);
            var guardado = await _context.SaveChangesAsync() > 0;
            return guardado 
                ? ResultadoOperacion.Ok() 
                : ResultadoOperacion.Fallo("Error al marcar la asistencia.");
        }
        public async Task<List<Inscripcion>> ObtenerPorUsuarioyFecha(int usuario, DateTime fecha)
        {
            return await QueryBase()
                .Where(i => i.idusuario == usuario && i.fecha.Date == fecha.Date)
                .ToListAsync();
        }

        public async Task<List<Inscripcion>> ObtenerIngresosPorFecha(DateTime desde, DateTime hasta)
        {
            return await QueryBase()
                .Where(i => i.asistio 
                        && i.fecha_checkin.HasValue 
                        && i.fecha_checkin.Value.Date >= desde.Date 
                        && i.fecha_checkin.Value.Date <= hasta.Date)
                .OrderByDescending(i => i.fecha_checkin)
                .ToListAsync();
        }
        public async Task<int> CantidadPorDia(DateTime desde, DateTime hasta)
        {
            return await QueryBase()
                .Where(i => i.asistio 
                        && i.fecha_checkin.HasValue 
                        && i.fecha_checkin.Value.Date >= desde.Date 
                        && i.fecha_checkin.Value.Date <= hasta.Date)
                .Select(i => i.idusuario)
                .Distinct()
                .CountAsync();
        }
        public async Task<int> ContarInscriptosPorClaseYFecha(int idClase, DateTime fecha)
        {
            return await _context.Inscripciones
                .CountAsync(i => i.idclase == idClase && i.fecha.Date == fecha.Date);
        }
        public async Task<List<Inscripcion>> ObtenerInscripcionesPorFecha(DateTime desde, DateTime hasta)
        {
            return await QueryBase()
                .Where(i => i.fecha.Date >= desde.Date && i.fecha.Date <= hasta.Date)
                .OrderByDescending(i => i.fecha_checkin)
                .ToListAsync();
        }
        public async Task<(List<Inscripcion> Items, int Total)> ObtenerPaginado(
            int pagina, int pageSize, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var query = QueryBase();

            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                query = query.Where(i => i.fecha.Date >= fechaInicio.Value.Date 
                                    && i.fecha.Date <= fechaFin.Value.Date);
            }

            // Contamos el total ANTES de paginar (para calcular TotalPaginas)
            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(i => i.fecha)
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
        public async Task<int> ContarInscriptosDelDia(int idClase, DateTime fecha)
        {
            return await _context.Inscripciones
                .CountAsync(i => i.idclase == idClase && i.fecha.Date == fecha.Date);
        }

        public async Task<ResultadoOperacion> CancelarClaseDelDia(int idClase, DateTime fecha)
        {
            var inscripciones = await _context.Inscripciones
                .Where(i => i.idclase == idClase && i.fecha.Date == fecha.Date)
                .ToListAsync();

            if (inscripciones.Any())
                _context.Inscripciones.RemoveRange(inscripciones);

            var yaCerrada = await _context.ClaseCierres
                .AnyAsync(c => c.idclase == idClase && c.fecha.Date == fecha.Date);

            if (!yaCerrada)
                _context.ClaseCierres.Add(new ClaseCierre { idclase = idClase, fecha = fecha.Date });

            var guardado = await _context.SaveChangesAsync() >= 0; // >= 0 porque puede no haber inscriptos que borrar
            return ResultadoOperacion.Ok();
        }

        public async Task<ResultadoOperacion> CerrarCupos(int idClase, DateTime fecha)
        {
            var yaExiste = await _context.ClaseCierres
                .AnyAsync(c => c.idclase == idClase && c.fecha.Date == fecha.Date);

            if (yaExiste)
                return ResultadoOperacion.Fallo("Los cupos ya se encuentran cerrados para hoy.");

            _context.ClaseCierres.Add(new ClaseCierre { idclase = idClase, fecha = fecha.Date });
            var guardado = await _context.SaveChangesAsync() > 0;

            return guardado
                ? ResultadoOperacion.Ok()
                : ResultadoOperacion.Fallo("Error al cerrar los cupos.");
        }

        public async Task<bool> EstaCerrada(int idClase, DateTime fecha)
        {
            return await _context.ClaseCierres
                .AnyAsync(c => c.idclase == idClase && c.fecha.Date == fecha.Date);
        }
        public async Task<List<Inscripcion>> ObtenerInscriptosDeClaseYFecha(int idClase, DateTime fecha)
        {
            return await QueryBase()
                .Where(i => i.idclase == idClase && i.fecha.Date == fecha.Date)
                .OrderBy(i => i.Usuario!.nombre)
                .ToListAsync();
        }
    }
}