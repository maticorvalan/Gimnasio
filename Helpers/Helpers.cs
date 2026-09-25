public class ResultadoOperacion
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = string.Empty;

    public static ResultadoOperacion Ok() => 
        new() { Exito = true };
    
    public static ResultadoOperacion Fallo(string mensaje) => 
        new() { Exito = false, Mensaje = mensaje };
}