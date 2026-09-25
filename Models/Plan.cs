using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gimnasio.Models
{
    [Table("planes")]
    public class Plan
    {
        public int id { get; set; }
        
        [Required(ErrorMessage = "El nombre del plan es obligatorio")]
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        
        // Usamos un int que puede ser nulo (int?). 
        // Si es 8 o 12, se limita. Si es NULL, significa "Pase Libre" (ilimitado).
        public int? limite_clases { get; set; } 
        
        [Required(ErrorMessage = "El precio es obligatorio")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal precio { get; set; }

        public ICollection<Suscripcion> Suscripciones { get; set; } = new List<Suscripcion>();
    }
}