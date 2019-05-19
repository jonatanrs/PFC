using System.Collections.Generic;
using System.IO;

namespace PFC.WebApp.Services.DocumentManager
{
    /// <summary>
    /// Hace referencia a un repositorio (carpeta) del gestor documental.
    /// </summary>
    public interface IDocumentRepository
    {
        /// <summary>
        /// Identificador único del repositorio (carpeta)
        /// </summary>
        string RepositoryId { get; }

        /// <summary>
        /// Lista de documentos contenidos en el repositorio
        /// </summary>
        /// <returns></returns>
        IEnumerable<IDocument> ListDocuments();

        /// <summary>
        /// Obtiene un documento específico del repositorio
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IDocument GetDocument(string name);

        /// <summary>
        /// Crea el documento con el id pasado como parámetro.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        IDocument CreateDocument(string name, Stream stream);

        /// <summary>
        /// Actualiza el documento con el id pasado como parámetro.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        IDocument UpdateDocument(string name, Stream stream);

        /// <summary>
        /// Elimina el documento con el id pasado como parámetro.
        /// </summary>
        /// <param name="name"></param>
        void DeleteDocument(string name);

        /// <summary>
        /// Renombramos el documento
        /// </summary>
        /// <param name="name">Identificador del documento</param>
        /// <param name="newName">Nuevo nombre para el documento</param>
        void RenameDocument(string name, string newName);
    }
}