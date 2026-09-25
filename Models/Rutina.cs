using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gimnasio.Models
{
    [Table("rutinas")]
    public class Rutina
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Debe ingresar un título para la rutina")]
        public string titulo { get; set; } = string.Empty;

        public string descripcion { get; set; } = string.Empty;

        public string ruta_archivo { get; set; } = string.Empty; 

        public DateTime fecha_asignacion { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Debe seleccionar el alumno")]
        public int idusuario { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el profesor")]
        public int idprofesor { get; set; }

        // Navegación
        [ForeignKey(nameof(idusuario))]
        public Usuario? Usuario { get; set; }

        [ForeignKey(nameof(idprofesor))]
        public Profesor? Profesor { get; set; }
    }
}