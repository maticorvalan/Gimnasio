using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Gimnasio.Data;
using Gimnasio.Models;
using Gimnasio.Models.ViewModels;
using Gimnasio.Models.DTOs;
using Gimnasio.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Gimnasio.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ImagenService _imagenService;
        private readonly IClaseService _claseService;
        private readonly IInscripcionService _inscripcionService;
        private readonly ISuscripcionService _suscripcionService;
        private readonly IPlanService _planService;

        public UsuariosController(IUsuarioService usuarioService, ImagenService imagenService,
            IClaseService claseService, IInscripcionService inscripcionService, ISuscripcionService suscripcionService,
             IPlanService planService)
        {
            _usuarioService = usuarioService;
            _imagenService = imagenService;
            _claseService = claseService;
            _inscripcionService = inscripcionService;
            _suscripcionService = suscripcionService;
            _planService = planService;
        }

        // GET: Usuarios
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index(int pagina = 1, int pageSize = 5, string? q = null)
        {
            if (pagina < 1) pagina = 1;
            if (pageSize < 1) pageSize = 5;

            var resultado = await _usuarioService.ObtenerPaginado(pagina, pageSize, q);

            ViewBag.PaginaActual = pagina;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRegistros = resultado.Total;
            ViewBag.TotalPaginas = Math.Max(1, (int)Math.Ceiling(resultado.Total / (double)pageSize));
            ViewBag.Query = q ?? string.Empty;

            return View(resultado.Items);
        }

        // GET: Usuarios/Details/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _usuarioService.ObtenerPorId(id.Value);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuarios/Create
        // [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            ViewBag.Roles = Usuario.ObtenerRoles();
            return View();
        }

        // POST: Usuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create([Bind("id,nombre,dni,rol,password,estado,fecha_alta")] Usuario usuario, IFormFile? avatar)
        {
            if (ModelState.IsValid)
            {
                if (usuario.dni.Length != 8 ){
                    
                    ModelState.AddModelError("dni", "El DNI debe tener 8 dígitos.");
                    ViewBag.Roles = Usuario.ObtenerRoles();
                    return View(usuario);
                }
                if(await _usuarioService.ObtenerPorDni(usuario.dni) != null)
                {
                    ModelState.AddModelError("dni", "El DNI ya está registrado.");
                    ViewBag.Roles = Usuario.ObtenerRoles();
                    return View(usuario);
                }
                var creado = await _usuarioService.Crear(usuario);
                if (!creado.Exito)
                {
                    ModelState.AddModelError(string.Empty, creado.Mensaje);
                    ViewBag.Roles = Usuario.ObtenerRoles();
                    return View(usuario);
                }
                if(avatar != null)
                {
                    var ruta = await _imagenService.GuardarImagen(avatar, "usuarios", usuario.id);
                    if(!string.IsNullOrEmpty(ruta))
                    {
                        usuario.ruta_avatar = ruta;
                        await _usuarioService.Actualizar(usuario);
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Roles = Usuario.ObtenerRoles();
            return View(usuario);
        }
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CreateAlumn()
        {
            ViewBag.Planes = await _planService.ObtenerTodos();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CreateAlumn([Bind("id,observaciones,nombre,dni")] Usuario usuario, int idPlan)
        {
            ViewBag.Planes = await _planService.ObtenerTodos();
            var plan = await _planService.ObtenerPorId(idPlan);
            if (plan == null)
            {
                ModelState.AddModelError("idPlan", "El plan seleccionado no es válido.");
                return View(usuario);
            }

            ModelState.Remove(nameof(Usuario.password));

            if (string.IsNullOrWhiteSpace(usuario.dni))
            {
                ModelState.AddModelError("dni", "El DNI es obligatorio.");
                return View(usuario);
            }

            if (usuario.dni.Length != 8)
            {
                ModelState.AddModelError("dni", "El DNI debe tener 8 dígitos.");
                return View(usuario);
            }

            if (await _usuarioService.ObtenerPorDni(usuario.dni) != null)
            {
                ModelState.AddModelError("dni", "El DNI ya está registrado.");
                return View(usuario);
            }

            var dni = usuario.dni.Trim();
            usuario.password = dni.Length >= 4
                ? dni.Substring(dni.Length - 4)
                : dni.PadLeft(4, '0');

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Error en el modelo: " + ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault());
                return View(usuario);
            }

            var creado = await _usuarioService.CrearAlumno(usuario, idPlan);
                if (!creado.Exito)
                {
                    ModelState.AddModelError(string.Empty, creado.Mensaje);
                    return View(usuario);
                }

                return RedirectToAction(nameof(Index));
            
        }

        // GET: Usuarios/Edit/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _usuarioService.ObtenerPorId(id.Value);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int id, [Bind("id,nombre,observaciones,dni,rol,password,estado,fecha_alta,ruta_avatar")] Usuario usuario, IFormFile? avatar, string? nuevaPassword)
        {
            if (id != usuario.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var usuarioActual = await _usuarioService.ObtenerPorId(usuario.id);
                    if (avatar != null && avatar.Length > 0)
                    {
                        var ruta = await _imagenService.GuardarImagen(avatar, "usuarios", usuario.id);
                        if(!string.IsNullOrEmpty(ruta))
                        {
                            usuario.ruta_avatar = ruta;
                        }
                    }
                    else
                    {
                        usuario.ruta_avatar = usuarioActual.ruta_avatar;
                    }
                    if (avatar != null && avatar.Length > 0)
{
                        // Le pasamos profesorActual.rutaFoto para que el servicio la elimine
                        var ruta = await _imagenService.GuardarImagen(avatar, "profesores", usuario.id, usuarioActual.ruta_avatar ?? string.Empty);
                        
                        if(!string.IsNullOrEmpty(ruta))
                        {
                            usuario.ruta_avatar = ruta;
                        }
                    }
                    else
                    {
                        usuario.ruta_avatar = usuarioActual.ruta_avatar;
                    }

                    if (!string.IsNullOrEmpty(nuevaPassword)) usuario.password = nuevaPassword;
                    var actualizado = await _usuarioService.Actualizar(usuario);
                    if (!actualizado.Exito)
                    {
                        TempData["Error"] = actualizado.Mensaje;
                        ModelState.AddModelError(string.Empty, actualizado.Mensaje);
                        return View(usuario);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await UsuarioExists(usuario.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // GET: Usuarios/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _usuarioService.ObtenerPorId(id.Value);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _usuarioService.ObtenerPorId(id);
            if (usuario != null)
            {
                var eliminado = await _usuarioService.Eliminar(id);
                if (!eliminado.Exito)
                {
                    TempData["ErrorMessage"] = eliminado.Mensaje;
                }
            }

            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "Administrador")]
        private async Task<bool> UsuarioExists(int id)
        {
            return await _usuarioService.ObtenerPorId(id) != null;
        }

        [HttpGet("api/[controller]")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GetUsuarios()
        {
            var lista = await _usuarioService.ObtenerTodos();
            return Ok(lista);
        }
        [HttpGet("api/Usuarios/buscar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Buscar([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return BadRequest("Ingrese al menos 2 caracteres.");

            var resultados = await _usuarioService.BuscarPorNombreODni(q);
            return Ok(resultados);
        }
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
                
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginView model)
        {
            try
            {
                if(!ModelState.IsValid)
                    return View(model);
                var e = await _usuarioService.ObtenerPorDni(model.Dni);
                if(e == null || !_usuarioService.VerifPassword(model.Password, e.password))
                {
                    ModelState.AddModelError("", "DNI o contraseña incorrectos.");
                    return View(model);
                }
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, e.id.ToString()),
                    new Claim(ClaimTypes.Name, e.nombre),
                    new Claim(ClaimTypes.Role, e.RolNombre),
                    new Claim("Avatar", string.IsNullOrEmpty(e.ruta_avatar) ? "/uploads/defecto.webp" : e.ruta_avatar)
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                


                await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity));
                if (e.rol == 2)
                    return RedirectToAction("Inicio", "Usuarios");
                else
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }
        [Route("salir", Name = "Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
        [Authorize(Roles = "Usuario,Administrador")]
        public async Task<IActionResult> Inicio(DateTime? fecha)
        {
            var idUsuario = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            var fechaSeleccionada = fecha ?? DateTime.Today; // Por defecto hoy

            // Nombre del día en español para filtrar clases
            var diasSemana = new Dictionary<DayOfWeek, string>
            {
                { DayOfWeek.Monday,    "LUN" },
                { DayOfWeek.Tuesday,   "MAR" },
                { DayOfWeek.Wednesday, "MIE" },
                { DayOfWeek.Thursday,  "JUE" },
                { DayOfWeek.Friday,    "VIE" },
                { DayOfWeek.Saturday,  "SAB" },
                { DayOfWeek.Sunday,    "DOM" }
            };
            var diaSemana = diasSemana[fechaSeleccionada.DayOfWeek];

            // Suscripción activa del usuario
            Suscripcion? suscripcion = null;
            try
            {
                suscripcion = await _suscripcionService.ObtenerSuscripcionActiva(idUsuario);
            }
            catch { } // Si no tiene suscripción activa, queda null

            var clasesDelDia = await _claseService.ObtenerPorDia(diaSemana);

            var inscriptosPorClase = new Dictionary<int, int>();
            foreach (var clase in clasesDelDia)
            {
                var cantidad = await _inscripcionService.ContarInscriptosPorClaseYFecha(clase.id, fechaSeleccionada);
                inscriptosPorClase[clase.id] = cantidad;
            }

            // Inscripciones del usuario hoy
            var misClasesHoy = await _inscripcionService.ObtenerPorUsuarioyFecha(idUsuario, fechaSeleccionada);

            // IDs de clases en las que ya está inscripto ese día
            var clasesInscriptas = misClasesHoy.Select(i => i.idclase).ToList();
            var clasesCerradas = new HashSet<int>();
            foreach (var clase in clasesDelDia)
            {
                var cerrada = await _inscripcionService.EstaCerrada(clase.id, fechaSeleccionada);
                if (cerrada) clasesCerradas.Add(clase.id);
            }

            var viewModel = new InicioView
            {
                Suscripcion       = suscripcion,
                MisClasesHoy      = misClasesHoy,
                ClasesDelDia      = clasesDelDia,
                FechaSeleccionada = fechaSeleccionada,
                ClasesInscriptas  = clasesInscriptas,   
                InscriptosPorClase = inscriptosPorClase,
                ClasesCerradas = clasesCerradas
            };

            return View(viewModel);
        }
        [HttpPost]
        [Authorize(Roles = "Usuario,Administrador")]
        public async Task<IActionResult> Inscribirse([FromBody] InscripcionDto dto)
        {
            var idUsuario = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            Suscripcion? suscripcion = null;
            try
            {
                suscripcion = await _suscripcionService.ObtenerSuscripcionActiva(idUsuario);
            }
            catch
            {
                return BadRequest(new { mensaje = "No tenés una suscripción activa." });
            }

            var inscripcionesHoy = await _inscripcionService.ObtenerPorUsuarioyFecha(idUsuario, DateTime.Today);
            var yaTieneClaseHoy = inscripcionesHoy.Any();

            if (!yaTieneClaseHoy && suscripcion.clases_restantes.HasValue && suscripcion.clases_restantes <= 0)
                return BadRequest(new { mensaje = "No tenés clases restantes en tu suscripción." });

            var inscripcion = new Inscripcion
            {
                idusuario = idUsuario,
                idclase   = dto.IdClase,
                fecha     = dto.Fecha.Date
            };

            var resultado = await _inscripcionService.Crear(inscripcion);

            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(new { mensaje = "Inscripción exitosa." });
        }
        [HttpPost]
        [Authorize(Roles = "Usuario,Administrador")]
        public async Task<IActionResult> CancelarInscripcion([FromBody] CancelarInscripcionDto dto)
        {
            var idUsuario = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var inscripcion = await _inscripcionService.ObtenerPorId(dto.IdInscripcion);

            if (inscripcion == null)
                return NotFound(new { mensaje = "No se encontró la inscripción." });

            var fechaHoraClase = inscripcion.fecha.Date + inscripcion.Clase!.hora_inicio;

            if (DateTime.Now > fechaHoraClase.AddHours(-2))
                return BadRequest(new { mensaje = "No se puede cancelar la inscripción a menos de 2 horas de la clase." });
            var resultado = await _inscripcionService.Eliminar(inscripcion.id);

            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(new { mensaje = "Inscripción cancelada exitosamente." });
        }
        [Authorize(Roles = "Usuario,Administrador")]
        public async Task<IActionResult> Perfil()
        {
                var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
            if (idUsuario == null || !int.TryParse(idUsuario, out var id))
            {
                return NotFound();
            }

            var usuario = await _usuarioService.ObtenerPorId(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Usuario,Administrador")]
        public async Task<IActionResult> Perfil(int id, [Bind("id,nombre,dni,rol,password,estado,fecha_alta,ruta_avatar")] Usuario usuario, IFormFile? avatar, string? nuevaPassword)
        {
            if (id != usuario.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var usuarioActual = await _usuarioService.ObtenerPorId(usuario.id);
                    
                    if (avatar != null && avatar.Length > 0)
                    {
                        var ruta = await _imagenService.GuardarImagen(avatar, "usuarios", usuario.id);
                        if(!string.IsNullOrEmpty(ruta))
                        {
                            usuario.ruta_avatar = ruta;
                        }
                    }
                    else
                    {
                        usuario.ruta_avatar = usuarioActual.ruta_avatar;
                    }

                    if (!string.IsNullOrEmpty(nuevaPassword)) usuario.password = nuevaPassword;
                    var actualizado = await _usuarioService.Actualizar(usuario);
                    if (!actualizado.Exito)
                    {
                        TempData["Error"] = actualizado.Mensaje;
                        ModelState.AddModelError(string.Empty, actualizado.Mensaje);
                        return View(usuario);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await UsuarioExists(usuario.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                if(usuario.rol == 2)
                    return RedirectToAction(nameof(Inicio));
                else
                {
                    return RedirectToAction("Index", "Home");
                }
                
            }
            return View(usuario);
        }
    }
}
