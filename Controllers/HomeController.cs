using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Gimnasio.Models;
using Gimnasio.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Gimnasio.Services;

namespace Gimnasio.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IClaseService _claseService;
    private readonly IInscripcionService _inscripcionService;
    private readonly IUsuarioService _usuarioService;
    private readonly ISuscripcionService _suscripcionService;

    public HomeController(ILogger<HomeController> logger, IClaseService claseService,
                IInscripcionService inscripcionService, IUsuarioService usuarioService, ISuscripcionService suscripcionService)
    {
        _logger = logger;
        _claseService = claseService;
        _inscripcionService = inscripcionService;
        _usuarioService = usuarioService;
        _suscripcionService = suscripcionService;
    }
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Index()
    {
        var hoy = DateTime.Today;
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
        
        ViewBag.Clases = await _claseService.ObtenerPorDia(diasSemana[hoy.DayOfWeek]);
        var inscriptosPorClase = new Dictionary<int, int>();
            foreach (var clase in ViewBag.Clases)
            {
                var cantidad = await _inscripcionService.ContarInscriptosPorClaseYFecha(clase.id, hoy);
                inscriptosPorClase[clase.id] = cantidad;
            }
        var clasesCerradas = new HashSet<int>();
        foreach (var clase in ViewBag.Clases)
        {
            var cerrada = await _inscripcionService.EstaCerrada(clase.id, hoy);
            if (cerrada) clasesCerradas.Add(clase.id);
        }
        ViewBag.ClasesCerradas = clasesCerradas;
        ViewBag.InscriptosPorClase = inscriptosPorClase;
        ViewBag.Ingresos = await _inscripcionService.ObtenerIngresosPorFecha(hoy, hoy);
        ViewBag.CantidadDia = await _inscripcionService.CantidadPorDia(hoy, hoy);
        ViewBag.CantidadUsuarios = await _usuarioService.CantidadAlumnos();
        return View();
    }
    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }
    [AllowAnonymous] // o el rol que corresponda según cómo la uses
    public IActionResult Ingreso()
    {
        return View();
    }
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> ConsultarPorDni([FromQuery] string dni)
    {
        var usuario = await _usuarioService.ObtenerPorDni(dni);
        if (usuario == null)
        {
            return Ok(new CheckInInfoDto
            {
                Encontrado = false,
                Mensaje = "No se encontró un usuario con ese DNI."
            });
        }

        Suscripcion? suscripcion;
        try
        {
            suscripcion = await _suscripcionService.ObtenerSuscripcionActiva(usuario.id);
        }
        catch
        {
            return Ok(new CheckInInfoDto
            {
                Encontrado = true,
                NombreUsuario = usuario.nombre,
                Mensaje = "El usuario no tiene una suscripción activa. Créditos insuficientes o cuota vencida."
            });
        }
        return Ok(new CheckInInfoDto
        {
            Encontrado = true,
            NombreUsuario = usuario.nombre,
            NombrePlan = suscripcion.Plan?.nombre,
            ClasesRestantes = suscripcion.clases_restantes,
            EsPaseLibre = !suscripcion.clases_restantes.HasValue,
            FechaVencimiento = suscripcion.fecha_fin

        });
    }
    [HttpGet]
    public async Task<IActionResult> InscriptosDelDia(int idClase)
    {
        var cantidad = await _inscripcionService.ContarInscriptosDelDia(idClase, DateTime.Today);
        return Ok(new { cantidad });
    }

    [HttpPost]
    public async Task<IActionResult> CancelarClaseHoy([FromBody] ClaseAccionDto dto)
    {
        var resultado = await _inscripcionService.CancelarClaseDelDia(dto.IdClase, DateTime.Today);

        return Ok(new
        {
            exito = resultado.Exito,
            mensaje = resultado.Exito ? "Clase cancelada y cupos cerrados para hoy." : resultado.Mensaje
        });
    }

    [HttpPost]
    public async Task<IActionResult> CerrarCuposHoy([FromBody] ClaseAccionDto dto)
    {
        var resultado = await _inscripcionService.CerrarCupos(dto.IdClase, DateTime.Today);

        return Ok(new
        {
            exito = resultado.Exito,
            mensaje = resultado.Exito ? "Cupos cerrados para hoy." : resultado.Mensaje
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public class ClaseAccionDto
    {
        public int IdClase { get; set; }
    }
    [HttpGet]
    public async Task<IActionResult> InscriptosDeClase(int idClase)
    {
        var inscripciones = await _inscripcionService.ObtenerInscriptosDeClaseYFecha(idClase, DateTime.Today);

        var resultado = inscripciones.Select(i => new
        {
            nombre = i.Usuario?.nombre ?? "Desconocido",
            dni = i.Usuario?.dni ?? "",
            asistio = i.asistio,
            horaCheckIn = i.fecha_checkin?.ToString("HH:mm")
        });

        return Ok(resultado);
    }
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> CheckInAutomatico([FromBody] CheckInDto dto)
    {
        var usuario = await _usuarioService.ObtenerPorDni(dto.Dni);
        if (usuario == null)
        {
            return Ok(new { exito = false, mensaje = "No se encontró un usuario con ese DNI." });
        }

        var clase = await _claseService.ObtenerClaseActual(DateTime.Now);
        if (clase == null)
        {
            return Ok(new { exito = false, mensaje = "No hay ninguna clase disponible en este horario." });
        }

        var resultado = await _claseService.MarcarAsistencia(clase.id, dto.Dni);

        if (!resultado.Exito)
        {
            return Ok(new { exito = false, mensaje = resultado.Mensaje });
        }

        Suscripcion? suscripcion = null;
        try
        {
            suscripcion = await _suscripcionService.ObtenerSuscripcionActiva(usuario.id);
        }
        catch { }

        var mensaje = $"¡Bienvenido, {usuario.nombre}!\n" +
                    $"Clase: {clase.nombre}\n" +
                    $"Horario: {clase.hora_inicio:hh\\:mm} - {clase.hora_fin:hh\\:mm}\n" +
                    $"Clases restantes: {(suscripcion?.clases_restantes?.ToString() ?? "Pase libre")}";

        return Ok(new { exito = true, mensaje = mensaje });
    }

    public class CheckInDto
    {
        public string Dni { get; set; } = string.Empty;
    }
}
