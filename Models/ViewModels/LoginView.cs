using System.ComponentModel.DataAnnotations;

namespace Gimnasio.Models.ViewModels
{
    public class LoginView
    {
        [Required(ErrorMessage = "Debe ingresar su DNI")]
        public string Dni { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar su contraseña")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool Recordarme { get; set; }
    }
}