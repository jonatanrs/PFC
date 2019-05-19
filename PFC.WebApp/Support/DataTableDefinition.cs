using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using PFC.WebApp.Support.PredicateBuilder;

namespace PFC.WebApp.Support
{
    public class DataTableDefinition
    {
        private readonly IDictionary<string, ColumnMap> columnMapping;
        private readonly IList<DataTableAction> globalActions;
        private readonly IList<DataTableAction> elementActions;
        private readonly Type _elementType;

        public IEnumerable<ColumnMap> Columns
            => columnMapping.Where(x => x.Value != null).Select(x => x.Value);

        public IEnumerable<DataTableAction> GlobalActions
            => globalActions;

        public IEnumerable<DataTableAction> ElementActions
            => elementActions;

        public string Title { get; private set; }

        public string ListAction { get; private set; }

        public DataTableDefinition(IQueryable<object> queryable) : this(queryable.ElementType) { }

        public static DataTableDefinition Create<T>()
        {
            return new DataTableDefinition(typeof(T));
        }

        public DataTableDefinition(Type elementType)
        {
            columnMapping = new Dictionary<string, ColumnMap>(elementType.GetProperties().Select(x => new KeyValuePair<string, ColumnMap>(x.Name, null)));
            globalActions = new List<DataTableAction>();
            elementActions = new List<DataTableAction>();
            _elementType = elementType;
        }

        public DataTableDefinition WithTitle(string title)
        {
            Title = title;
            return this;
        }

        public DataTableDefinition MapColumn(string column, string title, int width, string defaultOperator = null)
        {
            if (!columnMapping.ContainsKey(column))
                throw new KeyNotFoundException();

            if (defaultOperator == null)
            {
                var propertyType = _elementType.GetProperty(column).PropertyType;
                if (typeof(DateTime) == propertyType || typeof(int) == propertyType || typeof(decimal) == propertyType || typeof(bool) == propertyType || propertyType.IsEnum)
                    defaultOperator = "==";
                else
                    defaultOperator = "*=";

            }
            columnMapping[column] = new ColumnMap() { Column = column, Title = title, Width = width, DefaultOperator = defaultOperator };
            return this;
        }

        public DataTableDefinition AutoMapColumn()
        {
            foreach (var key in columnMapping.Keys.ToList())
                MapColumn(key, key, 100);

            return this;
        }

        public DataTableDefinition WithListAction(string listAction)
        {
            ListAction = listAction;
            return this;
        }

        public DataTableDefinition AddGlobalActions(string title, string url, string requiredRoles = null, DialogConfirmation dialogConfirmation = null)
        {
            globalActions.Add(new DataTableAction()
            {
                Title = title,
                Url = url,
                DialogConfirm = dialogConfirmation,
                RequiredRoles = requiredRoles
            });
            return this;
        }

        public DataTableDefinition AddElementAction(string title, string url, string requiredRoles = null, DialogConfirmation dialogConfirmation = null)
        {
            elementActions.Add(new DataTableAction()
            {
                Title = title,
                Url = url,
                DialogConfirm = dialogConfirmation,
                RequiredRoles = requiredRoles
            });
            return this;
        }

        public DataTableDefinition AddElementAction(string title, UrlActionDTO actionLinkDTO, string requiredRoles = null, DialogConfirmation dialogConfirmation = null)
        {
            elementActions.Add(new DataTableAction()
            {
                Title = title,
                UrlActionDTO = actionLinkDTO,
                DialogConfirm = dialogConfirmation,
                RequiredRoles = requiredRoles
            });
            return this;
        }

        /// <summary>
        /// Helper para contruir la respuesta para motanr los datos que van al cliente de DataTable.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="length">The length.</param>
        /// <param name="orderProperty">The order property.</param>
        /// <param name="inverse">if set to <c>true</c> [inverse].</param>
        /// <param name="query">The query.</param>
        /// <param name="queryable">The queryable.</param>
        /// <returns></returns>
        public static DataQuery DataQueryBuilder(int index, int length, string orderProperty, bool inverse, IEnumerable<QueryFilter> query, IQueryable<object> queryable, bool totalCount = true)
        {
            try
            {
                if (queryable == null)
                    throw new ArgumentNullException(nameof(queryable));

                string wherePredicate = PredicateBuilder.PredicateBuilder.Build(queryable.ElementType, query);
                if (!string.IsNullOrWhiteSpace(wherePredicate))
                    queryable = queryable.Where(wherePredicate);

                if (!string.IsNullOrEmpty(orderProperty))
                    queryable = inverse
                        ? queryable.OrderBy($"{orderProperty} DESC")
                        : queryable.OrderBy($"{orderProperty} ASC");

                var total = (totalCount)
                    ? queryable.Count()
                    : 0;

                queryable = queryable
                    .Skip(index * length)
                    .Take(length);

                return new DataQuery
                {
                    Success = true,
                    Total = total,
                    Elements = queryable.ToList()
                };
            }
            catch (Exception ex)
            {
                return new DataQuery
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public class UrlActionDTO
        {
            public string Controller { get; set; }
            public string Action { get; set; }
            public IDictionary<string, string> Values { get; set; }
        }
        public class DataTableAction
        {
            public string Url { get; internal set; }

            public DialogConfirmation DialogConfirm { get; internal set; }

            public string Title { get; internal set; }

            public UrlActionDTO UrlActionDTO { get; internal set; }

            public string RequiredRoles { get; set; }
        }

        public class DialogConfirmation
        {
            public string Mensaje { get; internal set; }
        }

        public class ColumnMap
        {
            public int Width { get; internal set; }
            public string Title { get; internal set; }
            public string Column { get; internal set; }
            public string DefaultOperator { get; set; }
        }
        public class DataQuery
        {
            public IEnumerable<object> Elements { get; set; }
            public int Total { get; set; }
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}
