using Gimnasio.Models;
using Microsoft.EntityFrameworkCore;
using Gimnasio.Data;

namespace Gimnasio.Services
{
    public class ClaseService : IClaseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUsuarioService _usuarioService;
        private readonly ISuscripcionService _suscripcionService;

        public ClaseService(ApplicationDbContext context, IUsuarioService usuarioService, ISuscripcionService suscripcionService)
        {
            _context = context;
            _usuarioService = usuarioService;
            _suscripcionService = suscripcionService;
        }
        private IQueryable<Clase> QueryBase()
        {
            return _context.Clases
                .Include(c => c.Profesor)
                .Include(c => c.Inscripciones);
        }
        public async Task<List<Clase>> ObtenerTodos()
        {
            return await QueryBase().ToListAsync();
        }

        public async Task<Clase?> ObtenerPorId(int id)
        {
            return await QueryBase()
                .FirstOrDefaultAsync(p => p.id == id);
        }

        public async Task<ResultadoOperacion> Crear(Clase clase)
        {
            _context.Clases.Add(clase);
            var guardado = await _context.SaveChangesAsync() > 0;
            return guardado 
                ? ResultadoOperacion.Ok() 
                : ResultadoOperacion.Fallo("Error al crear la clase.");
        }

        public async Task<ResultadoOperacion> Actualizar(Clase clase)
        {
            var claseExistente = await _context.Clases.FindAsync(clase.id);
            if (claseExistente == null)
                return ResultadoOperacion.Fallo("Clase no encontrada");

            claseExistente.nombre = clase.nombre;
            claseExistente.descripcion = clase.descripcion;
            claseExistente.dias_semana = clase.dias_semana;
            claseExistente.hora_inicio = clase.hora_inicio;
            claseExistente.hora_fin = clase.hora_fin;
            claseExistente.idprofesor = clase.idprofesor;
            claseExistente.capacidad = clase.capacidad;
            claseExistente.horario = clase.horario;

            _context.Clases.Update(claseExistente);
            var guardado = await _context.SaveChangesAsync() > 0;
            return guardado 
                ? ResultadoOperacion.Ok() 
                : ResultadoOperacion.Fallo("Error al actualizar la clase.");
        }

        public async Task<ResultadoOperacion> Eliminar(int id)
        {
            var clase = await _context.Clases.FindAsync(id);
            if (clase == null)
                return ResultadoOperacion.Fallo("Clase no encontrada");

            _context.Clases.Remove(clase);
            var guardado = await _context.SaveChangesAsync() > 0;
            return guardado 
                ? ResultadoOperacion.Ok() 
                : ResultadoOperacion.Fallo("Error al eliminar la clase.");
        }

        public async Task<int> ObtenerCantidad()
        {
            return await _context.Clases.CountAsync();
        }
        public async Task<ResultadoOperacion> MarcarAsistencia(int idClase, string dni)
        {
            var usuario = await _usuarioService.ObtenerPorDni(dni);
            if (usuario == null) return ResultadoOperacion.Fallo("Usuario no encontrado");

            Suscripcion? suscripcion;
            try
            {
                suscripcion = await _suscripcionService.ObtenerSuscripcionActiva(usuario.id);
            }
            catch
            {
                return ResultadoOperacion.Fallo("El usuario no tiene una suscripción activa");
            }

            var reservaExistente = await _context.Inscripciones
                .FirstOrDefaultAsync(i => i.idusuario == usuario.id 
                                        && i.idclase == idClase 
                                        && i.fecha.Date == DateTime.Today);

            if (reservaExistente != null)
            {
                if (reservaExistente.asistio)
                    return ResultadoOperacion.Fallo("El usuario ya registró su asistencia a esta clase hoy");

                reservaExistente.asistio = true;
                reservaExistente.fecha_checkin = DateTime.Now;

                var guardadoReserva = await _context.SaveChangesAsync() > 0;
                return guardadoReserva
                    ? ResultadoOperacion.Ok()
                    : ResultadoOperacion.Fallo("Error al guardar la asistencia.");
            }

            var asistioAlgoHoy = await _context.Inscripciones
                .AnyAsync(i => i.idusuario == usuario.id && i.fecha.Date == DateTime.Today && i.asistio);

            if (!asistioAlgoHoy && suscripcion.clases_restantes.HasValue && suscripcion.clases_restantes <= 0)
                return ResultadoOperacion.Fallo("El usuario no tiene clases restantes en su suscripción");

            if (!asistioAlgoHoy && suscripcion.clases_restantes.HasValue)
                await _suscripcionService.DescontarClase(suscripcion.id);

            var inscripcion = new Inscripcion {
                idusuario = usuario.id,
                idclase = idClase,
                asistio = true,
                fecha_checkin = DateTime.Now,
                fecha = DateTime.Now
            };
            _context.Inscripciones.Add(inscripcion);

            var guardado = await _context.SaveChangesAsync() > 0;
            return guardado 
                ? ResultadoOperacion.Ok() 
                : ResultadoOperacion.Fallo("Error al guardar la asistencia.");
        }
        public async Task<List<Clase>> ObtenerPorDia(string dia)
        {
            return await QueryBase()
                .Where(c => c.dias_semana.Contains(dia) && c.id != 1)
                .OrderBy(c => c.hora_inicio)
                .ToListAsync();
        }
        public async Task<Clase?> ObtenerClaseActual(DateTime ahora)
        {
            var horaActual = ahora.TimeOfDay;
            var margenTolerancia = TimeSpan.FromMinutes(10);

            var diasSemana = new Dictionary<DayOfWeek, string>
            {
                { DayOfWeek.Monday,    "LUN" },
                { DayOfWeek.Tuesday,   "MAR" },
                { DayOfWeek.Wednesday, "MIE" },
                { DayOfWeek.Thursday,  "JUE" },
                { DayOfWeek.Friday,    "VIE" },
                { DayOfWeek.Saturday,  "SAB" },
                { DayOfWeek.Sunday,    "DOM" }
            };
            var diaHoy = diasSemana[ahora.DayOfWeek];

            var clasesDelDia = await QueryBase()
                .Where(c => c.dias_semana.Contains(diaHoy))
                .ToListAsync();

            var clase = clasesDelDia.FirstOrDefault(c =>
                horaActual >= (c.hora_inicio - margenTolerancia) &&
                horaActual < c.hora_fin);

            return clase;
        }
    }
}