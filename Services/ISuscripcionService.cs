using Gimnasio.Models;
using Gimnasio.Models.DTOs;

namespace Gimnasio.Services
{
    public interface ISuscripcionService
    {
        Task<List<Suscripcion>> ObtenerTodos();
        Task<Suscripcion> ObtenerPorId(int id);
        Task<ResultadoOperacion> Crear(CrearSuscripcionDto dto);
        Task<ResultadoOperacion> Actualizar(Suscripcion suscripcion);
        Task<ResultadoOperacion> Eliminar(int id);
        Task<int> ObtenerCantidad();
        Task<ResultadoOperacion> DescontarClase(int idSuscripcion);
        Task<List<Suscripcion>> ObtenerSuscripcionesActivasPorUsuario(int idUsuario);
        Task<Suscripcion> ObtenerSuscripcionActiva(int idUsuario);
        Task<List<Suscripcion>> ObtenerSuscripcionessPorFecha(DateTime fecha);
        Task<(List<Suscripcion> Items, int Total)> ObtenerPaginado(
            int pagina, int pageSize, DateTime? fecha = null, 
            string? nombreUsuario = null, string? estado = null);
    }
}
