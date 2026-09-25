using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gimnasio.Models
{
    [Table("pagos")]
    public class Pago
    {
        public int id { get; set; }
        
        [Required(ErrorMessage = "Debe seleccionar un usuario")]
        public int idusuario { get; set; }
        public int? idsuscripcion { get; set; }
        
        [Required(ErrorMessage = "Debe ingresar un monto")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal monto { get; set; }
        
        [Required]
        public DateTime fecha_pago { get; set; }

        // Método de pago: "Efectivo", "Transferencia", "Tarjeta", etc.
        [MaxLength(50)]
        [Required(ErrorMessage = "Debe seleccionar un método de pago")]
        public string metodo_pago { get; set; } = "Efectivo";


        // Navegación
        [ForeignKey(nameof(idusuario))]
        public Usuario? Usuario { get; set; }

        [ForeignKey(nameof(idsuscripcion))]
        public Suscripcion? Suscripcion { get; set; }
    }
}