using Microsoft.EntityFrameworkCore;
using Gimnasio.Data;

namespace Gimnasio.Services
{
    public class SuscripcionLimpiadorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SuscripcionLimpiadorService> _logger;

        public SuscripcionLimpiadorService(IServiceProvider serviceProvider, ILogger<SuscripcionLimpiadorService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        var vencidas = await context.Suscripciones
                            .Where(s => s.activa &&
                                (s.fecha_fin < DateTime.Today ||
                                 (s.clases_restantes.HasValue && s.clases_restantes <= 0)))
                            .ToListAsync(stoppingToken);
                        var usuariosInactivos = await context.Usuarios
                            .Where(u => u.estado && !context.Suscripciones.Any(s => s.idusuario == u.id && s.activa))
                            .ToListAsync(stoppingToken);
                        foreach (var suscripcion in vencidas)
                        {
                            suscripcion.activa = false;
                        }
                        foreach (var usuario in usuariosInactivos)
                        {
                            usuario.estado = false;
                        }

                        if (vencidas.Any() || usuariosInactivos.Any())
                        {
                            await context.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation($"{vencidas.Count} suscripciones y {usuariosInactivos.Count} usuarios desactivados automáticamente.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al desactivar suscripciones vencidas.");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}