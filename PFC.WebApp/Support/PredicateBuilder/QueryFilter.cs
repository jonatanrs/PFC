using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFC.WebApp.Support.PredicateBuilder
{
    /// <summary>
    /// Filtro
    /// </summary>
    public class QueryFilter
    {
        /// <summary>
        /// Nombre de la propiedad sobre la que aplicar el filtro
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// Operador.
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// Valor
        /// </summary>
        public string Value { get; set; }
    }
}
