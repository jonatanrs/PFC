using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PFC.WebApp.Services.DocumentManager.Providers.DocumentManagerDiskProvider
{
    /// <summary>
    /// Gestor documental que utiliza el sistema de ficheros como almacenamiento
    /// </summary>
    /// <seealso cref="PFC.WebApp.Services.DocumentManager.IDocumentRepositoryManager" />
    public class DocumentRepositoryManager : IDocumentRepositoryManager
    {
        private string _path;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="DocumentRepositoryManager"/>.
        /// </summary>
        /// <param name="path">The path.</param>
        public DocumentRepositoryManager(string path)
        {
            _path = path;
        }

        /// <summary>
        /// Crea un nuevo repositorio con un identificador único.
        /// </summary>
        /// <returns></returns>
        public IDocumentRepository Create()
        {
            return Create(Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Crea el repositorio con nombre pasado como parámetro a menos que ya exista
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public IDocumentRepository Create(string identifier)
        {
            string path = Path.Combine(_path, identifier);
            Directory.CreateDirectory(path);
            return new DocumentRepository(path);
        }

        /// <summary>
        /// Borra el repositorio pasado como parámetro.
        /// </summary>
        /// <param name="identifier"></param>
        public void Delete(string identifier)
        {
            Directory.Delete(Path.Combine(_path, identifier), true);
        }

    }
}
