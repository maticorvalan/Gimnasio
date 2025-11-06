using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gimnasio.Models
{
    [Table("usuarios")]
    public class Usuario
    {
        public int id { get; set; }
        [Required]
        public string nombre { get; set; } = string.Empty;
        [Required]
        public string dni { get; set; } = string.Empty;
        [Required]
        public string rol { get; set; } = string.Empty;
        [Required]
        public string password { get; set; } = string.Empty;
        public bool estado { get; set; }
        public DateTime fecha_alta { get; set; }
        public string ruta_avatar { get; set; } = string.Empty;

        public ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();
    }
}