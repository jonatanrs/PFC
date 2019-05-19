using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PFC.WebApp.Services.DocumentManager
{
    /// <summary>
    /// Gestor de repositorios del gestor documental.
    /// </summary>
    public interface IDocumentRepositoryManager
    {
        /// <summary>
        /// Crea un nuevo repositorio con el identificador pasado como parámetro.
        /// </summary>
        /// <returns>El identificador del repositorio creado</returns>
        IDocumentRepository Create();
     
        /// <summary>
        /// Asegura que un repositorio existe y lo devuelve
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        IDocumentRepository Create(string identifier);

        /// <summary>
        /// Borra el repositorio pasado como parámetro.
        /// </summary>
        /// <param name="identifier"></param>
        void Delete(string identifier);
    }
}
