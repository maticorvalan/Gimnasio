using Gimnasio.Models;

namespace Gimnasio.Services
{
    public interface IProfesorService
    {
        Task<List<Profesor>> ObtenerTodos();
        Task<Profesor> ObtenerPorId(int id);
        Task<Profesor> ObtenerPorNombre(string nombre);
        Task<ResultadoOperacion> Crear(Profesor profesor);
        Task<ResultadoOperacion> Actualizar(Profesor profesor);
        Task<ResultadoOperacion> Eliminar(int id);
        Task<int> ObtenerCantidad();
    }
}
