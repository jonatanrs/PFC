using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PFC.WebApp.Services.DocumentManager.Providers.DocumentManagerDiskProvider
{
    /// <summary>
    /// Implementación en disco de un repositorio (carpeta) del gestor documental.
    /// </summary>
    /// <seealso cref="PFC.WebApp.Services.DocumentManager.IDocumentRepository" />
    internal class DocumentRepository : IDocumentRepository
    {
        private string _path;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="DocumentRepository"/>.
        /// </summary>
        /// <param name="identifier">Ruta del repositorio.</param>
        public DocumentRepository(string path)
        {
            _path = path;
        }

        /// <summary>
        /// Identificador único del repositorio (carpeta)
        /// </summary>
        public string RepositoryId => Path.GetDirectoryName(_path);

        /// <summary>
        /// Crea el documento con el id pasado como parámetro.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Ya existe un fichero con el mismo nombre</exception>
        public IDocument CreateDocument(string name, Stream stream)
        {
            string path = Path.Combine(_path, name);

            if (File.Exists(path))
                throw new InvalidOperationException("Ya existe un fichero con el mismo nombre");

            using (var destination = File.Open(path, FileMode.CreateNew, FileAccess.Write))
            {
                stream.CopyTo(destination);
            }
            return new Document(path);
        }

        /// <summary>
        /// Elimina el documento con el id pasado como parámetro.
        /// </summary>
        /// <param name="name"></param>
        public void DeleteDocument(string name)
        {
            string path = Path.Combine(_path, name);
            File.Delete(path);
        }

        /// <summary>
        /// Obtiene un documento específico del repositorio
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IDocument GetDocument(string name)
        {
            return new Document(Path.Combine(_path, name));
        }

        /// <summary>
        /// Lista de documentos contenidos en el repositorio
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IDocument> ListDocuments()
        {
            return Directory.GetFiles(_path)
                .Select(x => new Document(Path.Combine(_path, x)));
        }

        public void RenameDocument(string name, string newName)
        {
            Directory.Move(Path.Combine(_path, name), Path.Combine(_path, newName));
        }

        public IDocument UpdateDocument(string name, Stream stream)
        {
            string path = Path.Combine(_path, name);
            using (var destination = File.Open(path, FileMode.Truncate, FileAccess.Write))
            {
                stream.CopyTo(destination);
            }
            return new Document(path);
        }
    }
}