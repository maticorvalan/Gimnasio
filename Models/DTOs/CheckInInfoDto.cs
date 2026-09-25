

namespace Gimnasio.Models.DTOs
{
public class CheckInInfoDto
    {
        public bool Encontrado { get; set; }
        public string? Mensaje { get; set; }
        public string? NombreUsuario { get; set; }
        public string? NombrePlan { get; set; }
        public int? ClasesRestantes { get; set; }
        public bool EsPaseLibre { get; set; }
        public DateTime? FechaVencimiento { get; set; }
    }
}



