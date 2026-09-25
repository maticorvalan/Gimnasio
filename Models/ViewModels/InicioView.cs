namespace Gimnasio.Models.ViewModels
{
    public class InicioView
    {
        // Suscripción del usuario
        public Suscripcion? Suscripcion { get; set; }

        // Clases en las que está inscripto
        public List<Inscripcion> MisClasesHoy { get; set; } = new();

        // Clases disponibles del día
        public List<Clase> ClasesDelDia { get; set; } = new();

        // Fecha que está viendo el usuario
        public DateTime FechaSeleccionada { get; set; } = DateTime.Today;

        // IDs de clases en las que ya está inscripto
        public List<int> ClasesInscriptas { get; set; } = new();
        public Dictionary<int, int> InscriptosPorClase { get; set; } = new();
        public HashSet<int> ClasesCerradas { get; set; } = new();
    }
}