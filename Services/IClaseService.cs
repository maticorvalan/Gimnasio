using Gimnasio.Models;

namespace Gimnasio.Services
{
    public interface IClaseService
    {
        Task<List<Clase>> ObtenerTodos();
        Task<Clase?> ObtenerPorId(int id);
        Task<ResultadoOperacion> Crear(Clase clase);
        Task<ResultadoOperacion> Actualizar(Clase clase);
        Task<ResultadoOperacion> Eliminar(int id);
        Task<int> ObtenerCantidad();
        Task<List<Clase>> ObtenerPorDia(string dia);
        Task<ResultadoOperacion> MarcarAsistencia(int idClase, string dni);
        Task<Clase?> ObtenerClaseActual(DateTime ahora);
    }
}