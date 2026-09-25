using Gimnasio.Models;
using Gimnasio.Data;
using Microsoft.EntityFrameworkCore;

namespace Gimnasio.Services
{
    public class ProfesorService : IProfesorService
    {
        private readonly ApplicationDbContext _context;

        public ProfesorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Profesor>> ObtenerTodos()
        {
            return await _context.Profesores.ToListAsync();
        }

        public async Task<Profesor> ObtenerPorId(int id)
        {
            return await _context.Profesores.FindAsync(id) ?? throw new KeyNotFoundException($"No se encontró un profesor con ID {id}");
        }

        public async Task<Profesor> ObtenerPorNombre(string nombre)
        {
            return await _context.Profesores
                .FirstOrDefaultAsync(p => p.nombre == nombre) ?? throw new KeyNotFoundException($"No se encontró un profesor con nombre {nombre}");
        }

        public async Task<ResultadoOperacion> Crear(Profesor profesor)
        {
            _context.Profesores.Add(profesor);
            var guardado = await _context.SaveChangesAsync() > 0;
            return guardado
                ? ResultadoOperacion.Ok()
                : ResultadoOperacion.Fallo("Error al crear el profesor.");
        }

        public async Task<ResultadoOperacion> Actualizar(Profesor profesor)
        {
            try
            {
                var profesorExistente = await _context.Profesores.FindAsync(profesor.id);
                if (profesorExistente == null)
                    return ResultadoOperacion.Fallo($"No se encontró un profesor con ID {profesor.id}");

                profesorExistente.nombre = profesor.nombre;
                profesorExistente.especialidad = profesor.especialidad;
                if (!string.IsNullOrEmpty(profesor.rutaFoto))
                    profesorExistente.rutaFoto = profesor.rutaFoto;

                _context.Profesores.Update(profesorExistente);
                var guardado = await _context.SaveChangesAsync() > 0;
                return guardado
                    ? ResultadoOperacion.Ok()
                    : ResultadoOperacion.Fallo("Error al actualizar el profesor.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return ResultadoOperacion.Fallo($"Error al actualizar el profesor: {ex.Message}");
            }
        }

        public async Task<ResultadoOperacion> Eliminar(int id)
        {
            var profesor = await _context.Profesores.FindAsync(id);
            if (profesor == null)
                return ResultadoOperacion.Fallo($"No se encontró un profesor con ID {id}");

            _context.Profesores.Remove(profesor);
            var guardado = await _context.SaveChangesAsync() > 0;
            return guardado
                ? ResultadoOperacion.Ok()
                : ResultadoOperacion.Fallo("Error al eliminar el profesor.");
        }

        public async Task<int> ObtenerCantidad()
        {
            return await _context.Profesores.CountAsync();
        }
    }
}