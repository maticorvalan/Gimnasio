using Microsoft.EntityFrameworkCore;
using Gimnasio.Models;

namespace Gimnasio.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets - Representan las tablas
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Profesor> Profesores { get; set; } = null!;    
        public DbSet<Clase> Clases { get; set; } = null!;
        public DbSet<Inscripcion> Inscripciones { get; set; } = null!;
        public DbSet<Plan> Planes { get; set; }
        public DbSet<Suscripcion> Suscripciones { get; set; }
        public DbSet<Pago> Pagos { get; set; } = null!;
        public DbSet<ClaseCierre> ClaseCierres { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}