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

        // Navegación
        public Usuario Usuario { get; set; } = null!;
        public Clase Clase { get; set; } = null!;
    }
}