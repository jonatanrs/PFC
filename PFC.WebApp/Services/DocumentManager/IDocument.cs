using System;
using System.IO;

namespace PFC.WebApp.Services.DocumentManager
{
    /// <summary>
    /// Identifica un documento dentro del gestor documental.
    /// </summary>
    public interface IDocument
    {
        /// <summary>
        /// Nombre del documento.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Obtiene el stream de lectura del archivo
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">No existe la version del fichero</exception>
        Stream GetReadStream();
    }
}