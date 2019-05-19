using PFC.WebApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PFC.WebApp.Data.Models
{
    /// <summary>
    /// Entidad que representa el modelo de datos de un abono telefónico
    /// </summary>
    public class Subscription
    {
        /// <summary>
        /// Identificador.
        /// </summary>
        public virtual int ID { get; set; }

        [Required]
        public virtual string UserId { get; set; }

        /// <summary>
        /// Usuario al que pertenece el Abonado.
        /// </summary>
        public virtual ApplicationUser User { get; set; }

        /// <summary>
        /// Obtiene el nombre del usuario asociado al abonado
        /// </summary>
        /// <remarks>Se utiliza par mostrarlo en la DataTable</remarks>
        public virtual string UserName => User?.UserName ?? String.Empty;

        /// <summary>
        /// Número de teléfono (Mobile Station Integrated Services Digital Network.)
        /// </summary>
        [Required, MaxLength(15), Phone]
        public virtual string MSISDN { get; set; }

        /// <summary>
        /// Identificador de la tarifa asociada al abonado.
        /// </summary>
        public virtual int PlanId { get; set; }

        /// <summary>
        /// Propiedad de navegación hacia la tarifa asociada al abonado.
        /// </summary>
        [ForeignKey(nameof(PlanId))]
        public virtual Plan Plan { get; set; }

        /// <summary>
        /// Obtiene el nombre del plan.
        /// </summary>
        /// <remarks>Se utiliza par mostrarlo en la DataTable</remarks>
        public virtual string PlanName => Plan?.Name ?? String.Empty;

        /// <summary>
        /// Obtiene o establece la fecha de alta del abonado.
        /// </summary>
        [DataType(DataType.Date)]
        public virtual DateTime SubscriptionDate { get; set; }

        /// <summary>
        /// Obtiene o establece la fecha de baja.
        /// </summary>
        [DataType(DataType.Date)]
        public virtual DateTime? CancellationDate { get; set; }

        /// <summary>
        /// Obtiene las facturas para la subscripción actual.
        /// </summary>
        public virtual IList<Invoice> Invoices { get; set; }
    }
}
