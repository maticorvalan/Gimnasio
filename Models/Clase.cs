using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gimnasio.Models
{
    [Table("clases")]
    public class Clase
    {
        [Display(Name = "ID")]
        public int id { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string nombre { get; set; } = string.Empty;
        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string descripcion { get; set; } = string.Empty;
        public DateTime horario { get; set; }
        [Required(ErrorMessage = "Seleccione los días de la semana")]
        public string dias_semana { get; set; } = string.Empty;
        [Required(ErrorMessage = "Seleccione un horario de inicio")]
        public TimeSpan hora_inicio { get; set; }
        [Required(ErrorMessage = "Seleccione un horario de fin")]
        public TimeSpan hora_fin { get; set; }
        [Required(ErrorMessage = "El profesor es obligatorio")]
        public int idprofesor { get; set; }
        [Required(ErrorMessage = "La capacidad es obligatoria")]
        public int capacidad { get; set; } 
        // Navegación
        public ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();
        [ForeignKey(nameof(idprofesor))]
        public Profesor? Profesor { get; set; }
    }
}