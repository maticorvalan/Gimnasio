using Gimnasio.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Gimnasio.Services
{
    public class ImagenService
    {
        private readonly IWebHostEnvironment _env;

        public ImagenService(IWebHostEnvironment env)
        {
            _env = env;
        }

        // Agregamos rutaImagenAnterior como parámetro opcional
        public async Task<string> GuardarImagen(IFormFile archivo, string carpeta, int id, string rutaImagenAnterior = "")
        {
            if (archivo == null || archivo.Length == 0)
                return string.Empty;

            var extensiones = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(archivo.FileName).ToLower();
            
            if (!extensiones.Contains(extension))
                return string.Empty;

            // 1. Eliminar la imagen anterior si existe físicamente en el servidor
            if (!string.IsNullOrEmpty(rutaImagenAnterior))
            {
                // Convertimos la ruta web (/uploads/profesores/...) a ruta física del disco (C:\...\wwwroot\uploads\...)
                string rutaEliminar = Path.Combine(_env.WebRootPath, 
                    rutaImagenAnterior.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                
                if (System.IO.File.Exists(rutaEliminar))
                {
                    System.IO.File.Delete(rutaEliminar);
                }
            }

            // 2. Generar el nuevo nombre único usando Guid para evitar el caché
            var nombreArchivo = $"avatar_{id}_{Guid.NewGuid()}{extension}";

            var carpetaFisica = Path.Combine(_env.WebRootPath, "uploads", carpeta);
            if (!Directory.Exists(carpetaFisica))
            {
                Directory.CreateDirectory(carpetaFisica);
            }

            var rutaFisica = Path.Combine(carpetaFisica, nombreArchivo);

            // 3. Guardar la nueva imagen
            using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            return $"/uploads/{carpeta}/{nombreArchivo}";
        }
        public async Task<string> GuardarArchivo(IFormFile archivo, string carpeta, int idReferencia, string rutaArchivoAnterior = "")
        {
            if (archivo == null || archivo.Length == 0)
                return string.Empty;

            // Ahora permitimos PDFs y documentos
            var extensiones = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf", ".doc", ".docx" };
            var extension = Path.GetExtension(archivo.FileName).ToLower();
            
            if (!extensiones.Contains(extension))
                return string.Empty; // Podrías lanzar una excepción si suben un archivo no permitido

            if (!string.IsNullOrEmpty(rutaArchivoAnterior))
            {
                string rutaEliminar = Path.Combine(_env.WebRootPath, 
                    rutaArchivoAnterior.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (System.IO.File.Exists(rutaEliminar))
                    System.IO.File.Delete(rutaEliminar);
            }

            // Usamos el Guid para evitar problemas de caché, igual que con las imágenes
            var nombreArchivo = $"doc_{idReferencia}_{Guid.NewGuid()}{extension}";
            var carpetaFisica = Path.Combine(_env.WebRootPath, "uploads", carpeta);
            
            if (!Directory.Exists(carpetaFisica))
                Directory.CreateDirectory(carpetaFisica);

            var rutaFisica = Path.Combine(carpetaFisica, nombreArchivo);

            using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            return $"/uploads/{carpeta}/{nombreArchivo}";
        }
    }
}