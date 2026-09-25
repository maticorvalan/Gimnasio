using Gimnasio.Models;
using Microsoft.EntityFrameworkCore;

namespace Gimnasio.Services
{
        public interface IInscripcionService
        {
                Task<List<Inscripcion>> ObtenerTodos();
                Task<Inscripcion?> ObtenerPorId(int id);
                Task<ResultadoOperacion> Crear(Inscripcion inscripcion);
                Task<ResultadoOperacion> Eliminar(int id);
                Task<ResultadoOperacion> Actualizar(Inscripcion inscripcion);
                Task<int> ObtenerCantidad();
                Task<ResultadoOperacion> MarcarAsistencia(int idInscripcion, bool asistio);
                Task<List<Inscripcion>> ObtenerPorUsuarioyFecha(int usuario, DateTime fecha);
                Task<List<Inscripcion>> ObtenerIngresosPorFecha(DateTime desde, DateTime hasta);
                Task<int> CantidadPorDia(DateTime desde, DateTime hasta);
                Task<int> ContarInscriptosPorClaseYFecha(int idClase, DateTime fecha);
                Task<List<Inscripcion>> ObtenerInscripcionesPorFecha(DateTime desde, DateTime hasta);
                Task<(List<Inscripcion> Items, int Total)> ObtenerPaginado(
                        int pagina, int pageSize, DateTime? fechaInicio = null, DateTime? fechaFin = null);
                // IInscripcionService
                Task<int> ContarInscriptosDelDia(int idClase, DateTime fecha);
                Task<ResultadoOperacion> CancelarClaseDelDia(int idClase, DateTime fecha);
                Task<ResultadoOperacion> CerrarCupos(int idClase, DateTime fecha);
                Task<bool> EstaCerrada(int idClase, DateTime fecha);
                Task<List<Inscripcion>> ObtenerInscriptosDeClaseYFecha(int idClase, DateTime fecha);

        }
}