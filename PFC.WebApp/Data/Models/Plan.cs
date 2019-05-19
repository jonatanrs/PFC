using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PFC.WebApp.Data.Models
{
    /// <summary>
    /// Modelo de datos que represente una tarifa móvil
    /// </summary>
    public class Plan
    {
        /// <summary>
        /// Identificador.
        /// </summary>
        public virtual int ID { get; set; }

        /// <summary>
        /// Nombre del plan.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Especifica el coste del bono.
        /// </summary>
        public virtual decimal Charge { get; set; }

        /// <summary>
        /// Minutos en llamadas sin coste incluidos en línea.
        /// </summary>
        public virtual int VoicePlan { get; set; }

        /// <summary>
        /// Tráfico de datos sin coste de navegación incluidos en línea.
        /// </summary>
        public virtual int DataPlan { get; set; }

        /// <summary>
        /// SMS sin coste incluidos en línea.
        /// </summary>
        public virtual int SMSPlan { get; set; }

    }
}
