using Gimnasio.Models;

namespace Gimnasio.Services
{
   public interface IUsuarioService
    {
        Task<List<Usuario>> ObtenerTodos();
        Task<Usuario> ObtenerPorId(int id);
        Task<Usuario> ObtenerPorNombre(string nombre);
        Task<Usuario?> ObtenerPorDni(string dni);
        Task<ResultadoOperacion> Crear(Usuario usuario);
        Task<ResultadoOperacion> Actualizar(Usuario usuario);
        Task<ResultadoOperacion> Eliminar(int id);
        Task<int> ObtenerCantidad();
        Task<ResultadoOperacion> CrearAlumno(Usuario usuario, int idPlan);
        Task<IEnumerable<object>> BuscarPorNombreODni(string query);
        bool VerifPassword(string pass, string hash);
        Task<int> CantidadAlumnos();
        Task<(List<Usuario> Items, int Total)> ObtenerPaginado(
            int pagina, int pageSize, string? query = null);
    }
}