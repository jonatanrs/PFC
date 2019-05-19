using System.IO;

namespace PFC.WebApp.Services.DocumentManager.Providers.DocumentManagerDiskProvider
{
    /// <summary>
    /// Implementación en disco de un documento del gestor documental.
    /// </summary>
    internal class Document : IDocument
    {
        private string _path;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="Document"/>.
        /// </summary>
        /// <param name="path">Ruta del fichero.</param>
        public Document(string path)
        {
            _path = path;
        }

        /// <summary>
        /// Nombre del documento.
        /// </summary>
        public string Name => Path.GetFileName(_path);

        /// <summary>
        /// Obtiene el stream de lectura del archivo
        /// </summary>
        /// <returns></returns>
        public Stream GetReadStream()
        {
            return File.OpenRead(_path);
        }
    }
}