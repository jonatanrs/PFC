using PFC.WebApp.Data.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace PFC.WebApp.Models.MasMovilViewModels
{
    /// <summary>
    /// Modelo de datos que representa un registro de los ficheros de datos de llamadas del Proveedor de telefónía MásMóvil
    /// </summary>
    public class MasMovilEDR
    {
        /// <summary>
        /// Código de dealer-tienda.
        /// </summary>
        public string Dealer { get; set; }

        /// <summary>
        /// Fecha de exportación del fichero.
        /// </summary>
        public DateTime FechaExtraccion { get; set; }

        /// <summary>
        /// Fecha del evento (llamada, sms, mms, datos).
        /// </summary>
        public DateTime Fecha { get; set; }

        /// <summary>
        /// Fecha de cierre de facturación.
        /// Por ejemplo, tráfico del 15/05/2012 tendrá una fecha de factura de 20120601.
        /// </summary>
        [MaxLength(8)]
        public string Factura { get; set; }

        /// <summary>
        /// Número de teléfono origen (MÁSMÓVIL).
        /// </summary>
        [MaxLength(9)]
        public string MSISDN { get; set; }

        /// <summary>
        /// Tipología de tráfico.
        /// </summary>
        public string Tipo_Destino { get; set; }

        /// <summary>
        /// Destino, dos posibles valores:
        /// Número de teléfono destino en caso de tráfico de VOZ/SMS/MMS
        /// “DATA” en caso de tráfico de datos(GPRS).
        /// </summary>
        public string Destino { get; set; }

        /// <summary>
        /// Volumen de tráfico, dependiendo del tipo de tráfico:
        ///  - Número de segundos, en caso de VOZ.
        ///  - 0 en caso de SMS/MMS
        ///  - Kilobytes (KB) con dos decimales. Por ejemplo el consume de 512,00KB sera mostrado como 51200
        /// </summary>
        public int Minutos_Bytes { get; set; }

        /// <summary>
        /// Coste del evento. Impuestos indirectos no incluidos..
        /// </summary>
        public decimal Valor { get; set; }

        /// <summary>
        /// Impuestos indirectos asociados a valor (IVA, IPSI, IGIC).
        /// </summary>
        public decimal Impuestos { get; set; }

        /// <summary>
        /// Parsea un registro de MásMóvil.
        /// </summary>
        /// <param name="values">Los valores en el orden en el que están definidos por el proveedor.</param>
        /// <returns></returns>
        public static MasMovilEDR Parse(string[] values)
        {
            return new MasMovilEDR()
            {
                Dealer = values[0],
                FechaExtraccion = DateTime.Parse(values[1], new CultureInfo("es")),
                Fecha = DateTime.Parse(values[2], new CultureInfo("es")),
                Factura = values[3],
                MSISDN = values[4],
                Tipo_Destino = values[5],
                Destino = values[6],
                Minutos_Bytes = int.Parse(values[7], new CultureInfo("es")),
                Valor = decimal.Parse(values[8], new CultureInfo("es")),
                Impuestos = decimal.Parse(values[9], new CultureInfo("es"))
            };
        }

        /// <summary>
        /// Intenta parsear un registro de MásMóvil.
        /// </summary>
        /// <param name="values">Los valores en el orden en el que están definidos por el proveedor..</param>
        /// <param name="masMovilEDR">El registro parseado.</param>
        /// <param name="error">Un mensaje de error su el intento es fallido.</param>
        /// <returns></returns>
        public static bool TryParse(string[] values, out MasMovilEDR masMovilEDR, out string error)
        {
            try
            {
                if (values.Length != 10)
                {
                    error = "El número de columnas no coincide";
                    masMovilEDR = null;
                    return false;
                }

                masMovilEDR = Parse(values);
                error = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                masMovilEDR = null;
                error = ex.Message;
                return false;
            }
        }

        public PhoneEvent ToPhoneEvent(string localizer)
        {
            var result = new PhoneEvent()
            {
                Provider = "MasMovilProvider",
                Localizer = localizer,
                Date = Fecha,
                SourceId = MSISDN,
                ProviderCharge = Valor,
                Charge = Valor
            };

            switch (Tipo_Destino)
            {
                case "VOZ NACIONAL":
                    result.DestinationId = Destino;
                    result.Duration = Minutos_Bytes;
                    result.Type = PhoneEventType.Voz;
                    break;
                case "SMS NACIONAL":
                    result.DestinationId = Destino;
                    result.Duration = 0;
                    result.Type = PhoneEventType.SMS;
                    break;
                case "DATOS NACIONAL":
                    result.DestinationId = Destino;
                    result.Duration = Minutos_Bytes * 10;
                    result.Type = PhoneEventType.Datos;
                    break;
                default:
                    throw new NotImplementedException("No se han implementado el resto de tipos de eventos telefónicos para este proveedor");
            }

            return result;
        }
    }
}