using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Gimnasio.Models
{
    [Table("profesores")]
    public class Profesor
    {
        public int id { get; set; }
        [Required]
        public string nombre { get; set; } = string.Empty;
        [Required]
        public string especialidad { get; set; } = string.Empty;
        [Required]
        public string rutaFoto { get; set; } = string.Empty;


        public ICollection<Clase> Clases { get; set; } = new List<Clase>();
    }
}