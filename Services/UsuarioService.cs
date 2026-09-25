using Gimnasio.Data;
using Gimnasio.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gimnasio.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly ApplicationDbContext _context;

        public UsuarioService(ApplicationDbContext context)
        {
            _context = context;
        }
        private IQueryable<Usuario> QueryBase()
        {
            return _context.Usuarios
                .Include(u => u.Suscripciones)
                    .ThenInclude(s => s.Plan);
        }

        public async Task<List<Usuario>> ObtenerTodos()
        {
            return await QueryBase().ToListAsync();
        }

        public async Task<Usuario> ObtenerPorId(int id)
        {
            return await QueryBase().FirstOrDefaultAsync(u => u.id == id)
                ?? throw new KeyNotFoundException($"No se encontró un usuario con ID {id}");
        } 

        public async Task<Usuario> ObtenerPorNombre(string nombre)
        {
            return await QueryBase().FirstOrDefaultAsync(u => u.nombre == nombre)
                
                 ?? throw new KeyNotFoundException($"No se encontró un usuario con nombre {nombre}");
        }
        public async Task<Usuario?> ObtenerPorDni(string dni)
        {
            return await QueryBase().FirstOrDefaultAsync(u => u.dni == dni);
        }

        public async Task<ResultadoOperacion> Crear(Usuario usuario)
        {
            // Hash la contraseña
            usuario.password = HashPassword(usuario.password);
            usuario.fecha_alta = DateTime.Now;
            usuario.estado = true;

            _context.Usuarios.Add(usuario);
            var guardado = await _context.SaveChangesAsync() > 0;
            return guardado
                ? ResultadoOperacion.Ok()
                : ResultadoOperacion.Fallo("Error al crear el usuario.");
        }

        public async Task<ResultadoOperacion> CrearAlumno(Usuario usuario, int idPlan)
        {
            var plan = await _context.Planes.FindAsync(idPlan);
            if (plan == null)
                return ResultadoOperacion.Fallo($"Plan con ID {idPlan} no encontrado");
            usuario.fecha_alta = DateTime.Now;
            usuario.rol = 2; // Asignar rol de alumno
            usuario.estado = true;
            usuario.password = HashPassword(usuario.password);
            usuario.Suscripciones = new List<Suscripcion>
            {
                new Suscripcion
                {
                    idplan = idPlan,
                    fecha_inicio = DateTime.Now,
                    fecha_fin = DateTime.Now.AddMonths(1),
                    clases_restantes = plan.limite_clases,
                    activa = true
                }
            };
            _context.Usuarios.Add(usuario);
            var guardado = await _context.SaveChangesAsync() > 0;
            return guardado
                ? ResultadoOperacion.Ok()
                : ResultadoOperacion.Fallo("Error al crear el usuario.");
        }


        public async Task<ResultadoOperacion> Actualizar(Usuario usuario)
        {
            try
            {
                var existente = await _context.Usuarios.FindAsync(usuario.id);
                if (existente == null)
                    return ResultadoOperacion.Fallo($"No se encontró un usuario con ID {usuario.id}");

                if (!string.IsNullOrWhiteSpace(usuario.password) && existente.password != usuario.password)
                {
                    usuario.password = HashPassword(usuario.password);
                }
                else
                {
                    usuario.password = existente.password;
                }

                existente.nombre      = usuario.nombre;
                existente.dni         = usuario.dni;
                existente.rol         = usuario.rol;
                existente.password    = usuario.password;
                existente.estado      = usuario.estado;
                existente.fecha_alta  = usuario.fecha_alta;
                existente.observaciones = usuario.observaciones;
                if (!string.IsNullOrEmpty(usuario.ruta_avatar))
                    existente.ruta_avatar = usuario.ruta_avatar;

                var guardado = await _context.SaveChangesAsync() > 0;
                return guardado
                    ? ResultadoOperacion.Ok()
                    : ResultadoOperacion.Fallo("Error al actualizar el usuario.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return ResultadoOperacion.Fallo($"Error al actualizar el usuario: {ex.Message}");
            }
        }

        public async Task<ResultadoOperacion> Eliminar(int id)
        {
            var usuario = await QueryBase().FirstOrDefaultAsync(u => u.id == id);
            if (usuario == null)
                return ResultadoOperacion.Fallo($"No se encontró un usuario con ID {id}");

            _context.Usuarios.Remove(usuario);
            var guardado = await _context.SaveChangesAsync() > 0;
            return guardado
                ? ResultadoOperacion.Ok()
                : ResultadoOperacion.Fallo("Error al eliminar el usuario.");
        }


        public async Task<int> ObtenerCantidad()
        {
            return await _context.Usuarios.CountAsync();
        }

        // Hash de contraseña con PBKDF2
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    
        public async Task<IEnumerable<object>> BuscarPorNombreODni(string query)
        {
            return await _context.Usuarios
                .Where(u => u.nombre.Contains(query) || u.dni.Contains(query))
                .Take(10)
                .Select(u => new { u.id, u.nombre, u.dni })
                .ToListAsync<object>();
        }
        public bool VerifPassword(string pass, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(pass, hash);
        }

        public async Task<int> CantidadAlumnos()
        {
            return await _context.Usuarios.CountAsync(u => u.rol == 2);
        }
        public async Task<(List<Usuario> Items, int Total)> ObtenerPaginado(
            int pagina, int pageSize, string? query = null)
        {
            var baseQuery = QueryBase();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var termino = query.Trim();
                baseQuery = baseQuery.Where(u => u.nombre.Contains(termino) || u.dni.Contains(termino));
            }

            // Contamos el total ANTES de paginar (para calcular TotalPaginas)
            var total = await baseQuery.CountAsync();

            var items = await baseQuery
                .OrderByDescending(u => u.fecha_alta)
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}