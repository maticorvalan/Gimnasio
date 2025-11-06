using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gimnasio.Models
{
    [Table("clases")]
    public class Clase
    {
        public int id { get; set; }
        [Required]
        public string nombre { get; set; } = string.Empty;
        [Required]
        public string descripcion { get; set; } = string.Empty;
        [Required]
        public DateTime horario { get; set; }
        [Required]
        public int idprofesor { get; set; }
        [Required]
        public int capacidad { get; set; }

        // Navegación
        public ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();
        public Profesor Profesor { get; set; } = null!;
    }
}