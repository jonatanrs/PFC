using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PFC.WebApp
{
    /// <summary>
    /// Métodos de extension para los IEnumerables
    /// </summary>
    public static class IEnumerableExtensions
    {

        /// <summary>
        /// Realiza la acción especificada en cada elemento de la enumaración
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">La enumeración.</param>
        /// <param name="beforeAction">The before action.</param>
        /// <param name="afterAction">The after action.</param>
        /// <returns></returns>
        public static IEnumerable<T> ActionsForEach<T>(this IEnumerable<T> source, Action firstAction = null, Action<T> beforeAction = null, Action<T> afterAction = null, Action lastAcion = null)
        {
            if (source == null)
                yield break;

            firstAction?.Invoke();

            foreach (var item in source)
            {
                beforeAction?.Invoke(item);
                yield return item;
                afterAction?.Invoke(item);
            }

            lastAcion?.Invoke();
        }

        /// <summary>
        /// Actionses for each.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="firstAction">The first action.</param>
        /// <param name="beforeAction">The before action.</param>
        /// <param name="afterAction">The after action.</param>
        /// <param name="lastAcion">Acción que recibe el número de elementos procesados y se ejecuta al terminar la iteracion</param>
        /// <returns></returns>
        public static IEnumerable<T> ActionsForEach<T>(this IEnumerable<T> source, Action firstAction = null, Action<T, int> beforeAction = null, Action<T, int> afterAction = null, Action<int> lastAcion = null)
        {
            if (source == null)
                yield break;

            firstAction?.Invoke();

            int i = 0;
            foreach (var item in source)
            {
                beforeAction?.Invoke(item, i);
                yield return item;
                afterAction?.Invoke(item, i);
                i++;
            }

            lastAcion?.Invoke(i);

        }
    }
}
