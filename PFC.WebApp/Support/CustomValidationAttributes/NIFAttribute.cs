using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PFC.WebApp.Support.CustomValidationAttributes
{
    public class NIFAttribute : ValidationAttribute
    {
        private const string letrasNif = "TRWAGMYFPDXBNJZSQVHLCKE";

        public override string FormatErrorMessage(string name)
        {
            return $"El campo {name} no contiene un valor de NIF válido";
        }

        public override bool IsValid(object value)
        {
            if (!(value is string))
                throw new FormatException("El attributo de validación solo es aplicable a campos tipo string");

            return checkNIF(value as string);
        }

        private static bool checkNIF(string nif)
        {
            if (nif.Length != 9)
                return false;

            nif = nif.ToUpper();

            return ((nif[0] >= '0' && nif[0] <= '9') || nif[0] == 'X' || nif[0] == 'Y' || nif[0] == 'Z')
                ? checkPersonaFisica(nif)
                : checkOtherNif(nif);
        }

        private static bool checkOtherNif(string nif)
        {
            // El primer carácter debe ser alguno de entre las letras ABCDEFGHJKLMNPQRSUVW
            if (!"ABCDEFGHJKLMNPQRSUVW".Contains(nif[0]))
                return false;

            var digits = nif.Substring(1, 7);

            if (!digits.All(x => x >= '0' && x <= '9'))
                return false;

            var sum = digits.Select((x, i)
                => i % 2 == 0
                    ? (((x - 48) / 10) + ((x - 48) % 10))
                    : (x - 48))
                .Sum();
            sum = (10 - (sum % 10)) % 10;

            return "KLMNPQRSW".Contains(nif[0])
                ? nif[8] == "JABCDEFGHI"[sum]
                : nif[8] == sum;
        }

        private static bool checkPersonaFisica(string nif)
        {
            var numeroNif = nif.Substring(0, 8)
                .Replace("X", "0")
                .Replace("Y", "1")
                .Replace("Z", "2");

            var letra = nif[8];

            if (!int.TryParse(numeroNif, out int numero))
                return false;

            int i = numero % 23;
            return letra == letrasNif[i];
        }
    }
}
