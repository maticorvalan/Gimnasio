using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gimnasio.Models
{
    [Table("usuarios")]
    public class Usuario
    {
        public enum enRoles
        {
            Administrador = 1,
            Usuario = 2
        }


        public int id { get; set; }
        [Required(ErrorMessage = "Debe ingresar un nombre")]
        public string nombre { get; set; } = string.Empty;
        public string? observaciones { get; set; }
        [Required(ErrorMessage = "Debe ingresar un DNI")]
        public string dni { get; set; } = string.Empty;
        [Required]
        public int rol { get; set; }
        [Required]
        public string password { get; set; } = string.Empty;
        public bool estado { get; set; }
        public DateTime fecha_alta { get; set; }
        public string? ruta_avatar { get; set; } = string.Empty;

        public ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();
        public ICollection<Suscripcion> Suscripciones { get; set; } = new List<Suscripcion>();
        public ICollection<Rutina> Rutinas { get; set; } = new List<Rutina>();

        public string RolNombre => rol > 0 ? ((enRoles)rol).ToString() : "";

        public static IDictionary<int, string> ObtenerRoles()
		{
			SortedDictionary<int, string> roles = new SortedDictionary<int, string>();
			Type tipoEnumRol = typeof(enRoles);
			foreach (var valor in Enum.GetValues(tipoEnumRol))
			{
				roles.Add((int)valor, Enum.GetName(tipoEnumRol, valor) ?? "");
			}
			return roles;
		}

    }
}