


//Models/DTOs/CrearSuscripcionDto.cs

namespace Gimnasio.Models.DTOs
{

    public class CrearSuscripcionDto
    {
        public int idusuario { get; set; }
        public int idplan { get; set; }
        public string metodo_pago { get; set; } = "Efectivo";
    }
    public class CancelarInscripcionDto
    {
        public int IdInscripcion { get; set; }
    }
}