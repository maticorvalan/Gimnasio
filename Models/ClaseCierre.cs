// Models/ClaseCierre.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace Gimnasio.Models
{
    [Table("clase_cierres")]
    public class ClaseCierre
    {
        public int id { get; set; }
        public int idclase { get; set; }
        public DateTime fecha { get; set; }

        [ForeignKey(nameof(idclase))]
        public Clase? Clase { get; set; }
    }
}