using Gimnasio.Models;
using Microsoft.EntityFrameworkCore;
using Gimnasio.Data;

namespace Gimnasio.Services
{
    public class PlanService : IPlanService
    {
        private readonly ApplicationDbContext _context;

        public PlanService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<Plan>> ObtenerTodos()
        {
            return await _context.Planes.ToListAsync();
        }

        public async Task<Plan?> ObtenerPorId(int id)
        {
            return await _context.Planes.FindAsync(id);
        }

        public async Task<ResultadoOperacion> Crear(Plan plan)
        {
            _context.Planes.Add(plan);
            var guardado = await _context.SaveChangesAsync() > 0;
            return guardado 
                ? ResultadoOperacion.Ok() 
                : ResultadoOperacion.Fallo("Error al crear el plan.");
        }

        public async Task<ResultadoOperacion> Actualizar(Plan plan)
        {
            var planExistente = await _context.Planes.FindAsync(plan.id);
            if (planExistente == null)
                return ResultadoOperacion.Fallo($"No se encontró un plan con ID {plan.id}");

            planExistente.nombre = plan.nombre;
            planExistente.limite_clases = plan.limite_clases;
            planExistente.precio = plan.precio;

            _context.Planes.Update(planExistente);
            var guardado = await _context.SaveChangesAsync() > 0;
            return guardado 
                ? ResultadoOperacion.Ok() 
                : ResultadoOperacion.Fallo("Error al actualizar el plan.");
        }

        public async Task<ResultadoOperacion> Eliminar(int id)
        {
            var plan = await _context.Planes.FindAsync(id);
            if (plan == null)
                return ResultadoOperacion.Fallo($"No se encontró un plan con ID {id}");

            _context.Planes.Remove(plan);
            var guardado = await _context.SaveChangesAsync() > 0;
            return guardado 
                ? ResultadoOperacion.Ok() 
                : ResultadoOperacion.Fallo("Error al eliminar el plan.");
        }

        public async Task<int> ObtenerCantidad()
        {
            return await _context.Planes.CountAsync();
        }
    }
}