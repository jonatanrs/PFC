using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PFC.WebApp.Data.Models
{
    /// <summary>
    /// Enumeral con los estados de un fichero CSV de MásMóvil
    /// </summary>
    public enum MasMovilFileStateEnum
    {
        Nuevo,
        Importando,
        Importado,
        Error
    }

    /// <summary>
    /// Entidad para dar persistencia al estado de importación de un fichero CSV de MásMóvil
    /// </summary>
    public class MasMovilFileState
    {
        /// <summary>
        /// Obtiene o establece el nombre del fichero.
        /// </summary>
        [Key]
        public virtual string Name { get; set; }

        /// <summary>
        /// Obtiene o establece el estado en el que se encuentra el fichero.
        /// </summary>
        public virtual MasMovilFileStateEnum State { get; set; }

        [NotMapped]
        public virtual string StateString => State.ToString();

        /// <summary>
        /// Contiene el mensaje de error que ocurrió durante la importación.
        /// </summary>
        public virtual string Error { get; set; }

        /// <summary>
        /// Obtiene o establece el número de registros que contiene el fichero CSV.
        /// </summary>
        public virtual int RegistersCount { get; set; }
    }
}
