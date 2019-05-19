using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PFC.WebApp.Support.PredicateBuilder
{

    /// <summary>
    /// 
    /// </summary>
    public class PredicateBuilder
    {
        /// <summary>
        /// Obtiene un predicado de dinamiq linq a partir de un listado de queryfilter.
        /// </summary>
        /// <param name="filters">Filtros.</param>
        /// <returns></returns>
        /// <exception cref="System.MissingFieldException">El modelo no tiene la propiedad</exception>
        /// <exception cref="System.MissingMethodException">La propiedad no tiene los métodos Contains or StartsWith or EndsWith</exception>
        public static string Build<T>(IEnumerable<QueryFilter> filters, bool orConjunction = false)
        {
            return Build(typeof(T), filters, orConjunction);
        }

        /// <summary>
        /// Obtiene un predicado de dinamiq linq a partir de un listado de queryfilter.
        /// </summary>
        /// <param name="modelType">Tipo del modelo de datos.</param>
        /// <param name="filters">Filtros.</param>
        /// <param name="orConjunction">Si es verdadero, se hace aplica el operador OR para los filtros, por defecto se agrupan con AND.</param>
        /// <returns></returns>
        /// <exception cref="System.MissingFieldException">El modelo no tiene la propiedad</exception>
        /// <exception cref="System.MissingMethodException">La propiedad no tiene los métodos Contains or StartsWith or EndsWith</exception>

        public static string Build(Type modelType, IEnumerable<QueryFilter> filters, bool orConjunction = false)
        {
            if (filters == null)
                return "";

            var conjunction = orConjunction ? "OR" : "AND";

            return String.Join($" {conjunction} ", filters.Where(x => !string.IsNullOrWhiteSpace(x.Value)).Select(x => BuildFilter(modelType, x)));
        }

        private static string BuildFilter<T>(QueryFilter filter)
        {
            return BuildFilter(typeof(T), filter);
        }

        private static string BuildFilter(Type modelType, QueryFilter filter)
        {
            //Comprobar que existe el campo en el modelo.
            var campo = modelType.GetProperty(filter.Field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (campo == null)
                throw new MissingFieldException(modelType.Name, filter.Field);

            if(filter.Operator == "complex")
            {
                List<string> orsStrings = new List<string>();
                foreach (var orOp in filter.Value.ToString().ToLowerInvariant().Split(new string[] { "o" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    List<string> andsStrings = new List<string>();
                    foreach (var andOp in orOp.Split(new string[] { "y" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var op = andOp.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                        var o = op.Length == 1 ? "==" : op[0];
                        var v = op.Length == 1 ? op[0] : op[1];
                        andsStrings.Add(BuildFilter(modelType, new QueryFilter() { Field = filter.Field, Operator = o, Value = v}));
                    }
                    orsStrings.Add($"({string.Join(" AND ", andsStrings)})");
                }
                return $"({string.Join(" OR ", orsStrings)})";
            }

            //Comprobar que se puede convertir
            object valor = (campo.PropertyType.IsGenericType && campo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                ? Convert.ChangeType(filter.Value, Nullable.GetUnderlyingType(campo.PropertyType))
                : Convert.ChangeType(filter.Value, campo.PropertyType, CultureInfo.InvariantCulture);

            // En caso de ser un campo string lo envolvemos con las comillas
            if (campo.PropertyType == typeof(string))
                valor = $"\"{filter.Value.ToLowerInvariant()}\"";

            // En caso de ser un campo decimal le damos el formato correcto.
            else if (campo.PropertyType == typeof(decimal))
                valor = ((decimal)valor).ToString(CultureInfo.InvariantCulture);

            // En caso de ser un campo DateTime lo parseamos
            else if (campo.PropertyType == typeof(DateTime))
                valor = $"DateTime({((DateTime)valor).ToString("yyyy,MM,dd,HH,mm,ss")})";

            switch (filter.Operator)
            {
                case ">":
                case ">=":
                case "<":
                case "<=":
                case "!=":
                case "==":
                    return $"{filter.Field} {filter.Operator} {valor}";
                // Operadores válidos unicamente para String
                case "*=":
                    if (campo.PropertyType != typeof(string))
                        throw new MissingMethodException(campo.PropertyType.Name, "Contains");

                    return $"{filter.Field}.ToLowerInvariant().Contains({valor})";

                case "^=":
                    if (campo.PropertyType != typeof(string))
                        throw new MissingMethodException(campo.PropertyType.Name, "StartsWith");

                    return $"{filter.Field}.ToLowerInvariant().StartsWith({valor})";

                case "$=":
                    if (campo.PropertyType != typeof(string))
                        throw new MissingMethodException(campo.PropertyType.Name, "EndsWith");

                    return $"{filter.Field}.ToLowerInvariant().EndsWith({valor})";
                case "complex":
                    {
                        List<string> orsStrings = new List<string>();
                        foreach (var orOp in valor.ToString().ToLowerInvariant().Split(new string[] { "o" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            List<string> andsStrings = new List<string>();
                            foreach (var andOp in orOp.Split(new string[] { "y" }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var op = andOp.Split(new string[] { "y" }, StringSplitOptions.RemoveEmptyEntries);
                                andsStrings.Add($"{filter.Field} {op[0]} {op[1]}");
                            }
                            orsStrings.Add($"({string.Join(" AND ", andsStrings)})");
                        }
                        return $"({string.Join(" OR ", orsStrings)})";
                    }
                default:
                    throw new NotImplementedException("Operador no implementado");
            }
        }
    }
}
