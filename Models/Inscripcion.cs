using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Gimnasio.Models
{
    [Table("inscripciones")]
    public class Inscripcion
    {
        public int id { get; set; }
        [Required]
        public int idusuario { get; set; }
        [Required]
        public int idclase { get; set; }
        public DateTime fecha { get; set; }
        public bool asistio { get; set; } // Para marcar si el usuario asistió o no a la clase
        public DateTime? fecha_checkin { get; set; } // Para registrar la hora exacta de check-in
        // Navegación
        [ForeignKey(nameof(idusuario))]
        public Usuario? Usuario { get; set; }
        [ForeignKey(nameof(idclase))]
        public Clase? Clase { get; set; }
    }
}