using Gimnasio.Models;

namespace Gimnasio.Services
{
    public interface IPlanService
    {
        Task<List<Plan>> ObtenerTodos();
        Task<Plan?> ObtenerPorId(int id);
        Task<ResultadoOperacion> Crear(Plan plan);
        Task<ResultadoOperacion> Actualizar(Plan plan);
        Task<ResultadoOperacion> Eliminar(int id);
        Task<int> ObtenerCantidad();
    }
}