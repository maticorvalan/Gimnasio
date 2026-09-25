using Gimnasio.Models;

public interface IPagoService
{
    Task<IEnumerable<Pago>> ObtenerTodos();
    Task<Pago?> ObtenerPorId(int id);
    Task<ResultadoOperacion> CrearPago(Pago pago);
    Task<ResultadoOperacion> ActualizarPago(Pago pago);
    Task<ResultadoOperacion> EliminarPago(int id);
    Task<IEnumerable<Pago>> ObtenerPagosPorUsuario(int idusuario);
    Task<IEnumerable<Pago>> ObtenerPagosPorSuscripcion(int idsuscripcion);
    Task<IEnumerable<Pago>> ObtenerPagosPorFecha(DateTime fechaInicio, DateTime fechaFin);
    Task<IEnumerable<Pago>> ObtenerPagosPorMetodo(string metodo_pago);
    Task<(List<Pago> Items, int Total)> ObtenerPaginado(
                int pagina, int pageSize, DateTime? fecha = null);
    Task<decimal> PagoPorMes(int mes, int anio);
    Task<(decimal Total, int Cantidad)> ObtenerResumenMensual(int anio, int mes);
    
}