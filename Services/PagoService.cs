using Gimnasio.Data;
using Gimnasio.Models;
using Microsoft.EntityFrameworkCore;

namespace Gimnasio.Services
{
    public class PagoService : IPagoService
    {
        private readonly ApplicationDbContext _context;

        public PagoService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Incluimos Usuario y Suscripcion para tener todos los datos relacionados
        private IQueryable<Pago> QueryBase()
        {
            return _context.Pagos
                .Include(p => p.Usuario)
                .Include(p => p.Suscripcion!)
                    .ThenInclude(s => s.Plan);
        }

        public async Task<IEnumerable<Pago>> ObtenerTodos()
        {
            return await QueryBase().ToListAsync();
        }

        public async Task<Pago?> ObtenerPorId(int id)
        {
            return await QueryBase()
                .FirstOrDefaultAsync(p => p.id == id);
        }

        public async Task<ResultadoOperacion> CrearPago(Pago pago)
        {
                _context.Pagos.Add(pago);
                var guardado =await _context.SaveChangesAsync() > 0;
                return guardado 
                    ? ResultadoOperacion.Ok() 
                    : ResultadoOperacion.Fallo("Error al crear la clase.");

        }

        public async Task<ResultadoOperacion> ActualizarPago(Pago pago)
        {
            var pagoExistente = await _context.Pagos.FindAsync(pago.id);
            if (pagoExistente == null)
                return ResultadoOperacion.Fallo("Pago no encontrado.");
            pagoExistente.monto       = pago.monto;
            pagoExistente.metodo_pago = pago.metodo_pago;
            pagoExistente.idsuscripcion = pago.idsuscripcion;
            var guardado = await _context.SaveChangesAsync() > 0;
            return guardado
                ? ResultadoOperacion.Ok()
                : ResultadoOperacion.Fallo("Error al actualizar el pago.");
        }

        public async Task<ResultadoOperacion> EliminarPago(int id)
        {
            try
            {
                var pago = await _context.Pagos.FindAsync(id);
                if (pago == null) return ResultadoOperacion.Fallo("Pago no encontrado");

                _context.Pagos.Remove(pago);
                await _context.SaveChangesAsync();
                return ResultadoOperacion.Ok();
            }
            catch (Exception)
            {
                return ResultadoOperacion.Fallo("Error al eliminar el pago.");
            }
        }

        public async Task<IEnumerable<Pago>> ObtenerPagosPorUsuario(int idusuario)
        {
            return await QueryBase()
                .Where(p => p.idusuario == idusuario)
                .OrderByDescending(p => p.fecha_pago)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pago>> ObtenerPagosPorSuscripcion(int idsuscripcion)
        {
            return await QueryBase()
                .Where(p => p.idsuscripcion == idsuscripcion)
                .OrderByDescending(p => p.fecha_pago)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pago>> ObtenerPagosPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            return await QueryBase()
                .Where(p => p.fecha_pago >= fechaInicio && p.fecha_pago <= fechaFin)
                .OrderByDescending(p => p.fecha_pago)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pago>> ObtenerPagosPorMetodo(string metodo_pago)
        {
            return await QueryBase()
                .Where(p => p.metodo_pago == metodo_pago)
                .OrderByDescending(p => p.fecha_pago)
                .ToListAsync();
        }
        public async Task<(List<Pago> Items, int Total)> ObtenerPaginado(
            int pagina, int pageSize, DateTime? fecha = null)
        {
            var query = QueryBase();

            if (fecha.HasValue)
            {
                query = query.Where(p => p.fecha_pago.Date == fecha.Value.Date);
            }

            // Contamos el total ANTES de paginar (para calcular TotalPaginas)
            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.fecha_pago)
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
        public async Task<decimal> PagoPorMes(int mes, int anio)
        {
            return await _context.Pagos
                .Where(p => p.fecha_pago.Year == anio && p.fecha_pago.Month == mes)
                .SumAsync(p => p.monto);

        }
        public async Task<(decimal Total, int Cantidad)> ObtenerResumenMensual(int anio, int mes)
        {
            var pagosDelMes = _context.Pagos
                .Where(p => p.fecha_pago.Year == anio && p.fecha_pago.Month == mes);

            var total = await pagosDelMes.SumAsync(p => (decimal?)p.monto) ?? 0;
            var cantidad = await pagosDelMes.CountAsync();

            return (total, cantidad);
        }
    }
}