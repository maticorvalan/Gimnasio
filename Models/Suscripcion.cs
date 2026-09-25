using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gimnasio.Models
{
    [Table("suscripciones")]
    public class Suscripcion
    {
        public int id { get; set; }
        
        [Required(ErrorMessage = "Debe seleccionar un usuario")]
        public int idusuario { get; set; }
        
        [Required(ErrorMessage = "Debe seleccionar un plan")]
        public int idplan { get; set; }
        [Required(ErrorMessage = "Debe ingresar una fecha de inicio")]
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_fin { get; set; } // Para saber cuándo se le vence el mes
        
        // Aquí llevamos la cuenta. Si compró el de 8, empieza en 8 y va bajando.
        // Si el plan era Pase Libre (null), este campo también será null.
        public int? clases_restantes { get; set; } 
        
        public bool activa { get; set; } // Para saber si ya caducó o sigue vigente

        // Navegación
        [ForeignKey(nameof(idusuario))]
        public Usuario? Usuario { get; set; }
        
        [ForeignKey(nameof(idplan))]
        public Plan? Plan { get; set; }
    }
}