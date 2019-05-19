using PFC.WebApp.Models;
using PFC.WebApp.Support;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PFC.WebApp.Data.Models
{
    /// <summary>
    /// Entidad que representa el modelo de datos de una factura telefónica
    /// </summary>
    public class Invoice
    {
        /// <summary>
        /// Identificador de clave primaria para la base de datos.
        /// </summary>
        public virtual int ID { get; set; }


        public string ComercialId { get; set; }

        [ForeignKey(nameof(ComercialId))]
        public virtual ApplicationUser Comercial { get; set; }

        /// <summary>
        /// Identificador de factura.
        /// </summary>
        public virtual string Identificador => $"F{ID.ToString("D10")}";

        /// <summary>
        /// Identificador del abonado al que pertenece la factura.
        /// </summary>
        public virtual int SubscriptionId { get; set; }

        /// <summary>
        /// Ontiene o establece el abonado al que pertenece la factura
        /// </summary>
        [ForeignKey(nameof(SubscriptionId))]
        public virtual Subscription Subscription { get; set; }

        /// <summary>
        /// Fecha de inicio del período de facturación.
        /// </summary>
        [DataType(DataType.Date)]
        public virtual DateTime StartDate { get; set; }

        /// <summary>
        /// Fecha de fin del período de facturación.
        /// </summary>
        [DataType(DataType.Date)]
        public virtual DateTime EndDate { get; set; }

        /// <summary>
        /// Obtiene el mes del período de facturación.
        /// </summary>
        public virtual string MonthPeriod => StartDate.ToString("MMMM");

        /// <summary>
        /// Obtiene el año del período de facturación.
        /// </summary>
        public virtual string YearPeriod => StartDate.ToString("yyy");

        /// <summary>
        /// Fecha de emisión de la factura.
        /// </summary>
        [DataType(DataType.Date)]
        public virtual DateTime EmissionDate { get; set; }

        /// <summary>
        /// Indica cuando la factura está cobrada.
        /// </summary>
        public virtual bool Charged { get; set; }

        /// <summary>
        /// Ontiene el estado de la factura
        /// </summary>
        /// <remarks>
        /// Las facturas se generarán en estado provisional para poder verificarlas 
        /// Una vez verificadas, ya le aparezcan a clientes y/o comerciales.
        /// </remarks>
        public virtual InvoiceStateEnum State { get; set; }

        /// <summary>
        /// Gets the state string.
        /// </summary>
        [NotMapped]
        public virtual string StateString => State.ToString();

        public virtual decimal PlanPrice { get; set; }

        /// <summary>
        /// Indica la duración total de las llamadas del abonado durante el pedíodo de facturación.
        /// </summary>
        public virtual long CallDuration { get; set; }

        [NotMapped]
        public virtual string FriendlyCallDuration
            => TimeSpan.FromSeconds(CallDuration).ToString();

        /// <summary>
        /// Indica el coste generado por el consumo de llamadas durante el pedíodo de facturación.
        /// </summary>
        public virtual decimal CallPrice { get; set; }

        /// <summary>
        /// Indica el descuento aplicado sobre el coste de las llamadas en función de la tarifa vigente.
        /// </summary>
        public virtual decimal CallPriceDeduction { get; set; }

        /// <summary>
        /// Indica el tráfico de datos en bytes.
        /// </summary>
        public virtual long DataTrafficBytes { get; set; }

        [NotMapped]
        public virtual string FriendlyDataTraffic
            => FriendlyBytesFormat.ToFriendlyBytesFormat(DataTrafficBytes);

        /// <summary>
        /// Indica el coste generado por el tráfico de datos durante el pedíodo de facturación.
        /// </summary>
        public virtual decimal DataTrafficPrice { get; set; }

        /// <summary>
        /// Indica el descuento aplicado sobre el coste generado por el tráfico de datos en función de la tarifa vigente.
        /// </summary>
        public virtual decimal DataTrafficPriceDeduction { get; set; }

        /// <summary>
        /// Indica el número de mensajes de texto enviados durante el pedíodo de facturación.
        /// </summary>
        public virtual long SMS { get; set; }

        /// <summary>
        /// Indica el coste generado por el envío de mensajes de texto durante el pedíodo de facturación.
        /// </summary>
        public virtual decimal SMSPrice { get; set; }

        /// <summary>
        /// Indica el descuento aplicado sobre el coste generado por el envío de mensajes de texto en función de la tarifa vigente.
        /// </summary>
        public virtual decimal SMSPriceDeduction { get; set; }

        /// <summary>
        /// Indica el subtotal de la factura, sin aplicar los impuestos.
        /// </summary>
        public virtual decimal Subtotal { get; set; }

        /// <summary>
        /// Indica el tipo de impuesto aplicado a la factura. Valor expresado en % de 0 a 100.
        /// </summary>
        public virtual decimal TaxRate { get; set; }

        /// <summary>
        /// Indica el precio total de la factura (impuestos incluidos).
        /// </summary>
        public virtual decimal Total { get; set; }

        [NotMapped]
        public virtual IEnumerable<PhoneEvent> PhoneEvents { get; set; }


    }
}
