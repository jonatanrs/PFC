using PFC.WebApp.Support;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PFC.WebApp.Data.Models
{
    /// <summary>
    /// Entidad que representa un evento telefónico
    /// </summary>
    public class PhoneEvent
    {
        /// <summary>
        /// Identificador.
        /// </summary>
        public virtual long ID { get; set; }

        /// <summary>
        /// Identifica el evento telefónico en el proveedor de origen.
        /// </summary>
        public virtual string Localizer { get; set; }

        /// <summary>
        /// Identifica el proveedor que desde el que se obtuvo el evento telefónico.
        /// </summary>
        public virtual string Provider { get; set; }

        /// <summary>
        /// Obtiene el identificador de origen del evento
        /// </summary>
        [MaxLength(15), Phone]
        public virtual string SourceId { get; set; }

        /// <summary>
        /// Obtiene el identificador del destino del evento
        /// </summary>
        [MaxLength(15)]

        public virtual string DestinationId { get; set; }

        /// <summary>
        /// Obtiene el tipo de evento telefónico
        /// </summary>
        public virtual PhoneEventType Type { get; set; }

        /// <summary>
        /// Obtiene el valor en string del enumeral de tipo d eevento.
        /// </summary>
        public virtual string TypeString => Type.ToString();

        /// <summary>
        /// Obtiene la fecha en la que se produce el evento
        /// </summary>
        public virtual DateTime Date { get; set; }

        /// <summary>
        /// Obtiene la duración del evento
        /// </summary>
        public virtual long Duration { get; set; }

        /// <summary>
        /// Gets the time span.
        /// </summary>
        public virtual string FriendlyDuration
        {
            get
            {
                switch (Type)
                {
                    case PhoneEventType.Voz:
                        return TimeSpan.FromSeconds(Duration).ToString();
                    case PhoneEventType.Datos:
                        return FriendlyBytesFormat.ToFriendlyBytesFormat(Duration);
                    case PhoneEventType.SMS:
                    case PhoneEventType.MMS:
                    default:
                        return "";
                }
            }
        }

        /// <summary>
        /// Coste del evento telefónico proporcionado por el proveedor.
        /// </summary>
        public decimal ProviderCharge { get; set; }

        /// <summary>
        /// Coste del evento telefónico.
        /// </summary>
        public decimal Charge { get; set; }

        /// <summary>
        /// Obtiene el identificador de la factura con la que está asociado el evento telefónico.
        /// </summary>
        /// <remarks>No se establece como clave ajena para no perjudicar el rendimiento de esta tabla ya que va a contener muchos registros</remarks>
        public int InvoiceId { get; set; }
    }
}
