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
        [Required(ErrorMessage = "Debe ingresar un nombre")]
        public string nombre { get; set; } = string.Empty;
        [Required(ErrorMessage = "Debe ingresar una especialidad")]
        public string especialidad { get; set; } = string.Empty;
        public string? rutaFoto { get; set; } = string.Empty;


        public ICollection<Clase> Clases { get; set; } = new List<Clase>();
        public ICollection<Rutina> Rutinas { get; set; } = new List<Rutina>();
    }
}