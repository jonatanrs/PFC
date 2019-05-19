using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PFC.WebApp.Data;
using PFC.WebApp.Data.Models;
using PFC.WebApp.Support;
using PFC.WebApp.Support.PredicateBuilder;

namespace PFC.WebApp.Controllers
{
    [Authorize(Roles = "Comercial")]
    public class CommissionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommissionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            ViewBag.Title = "Comisiones";
        }

        public IActionResult Index()
        {
            ViewBag.SubTitle = "Listado";

            var dataTableDefinition = DataTableDefinition.Create<Invoice>()
                .WithTitle("Comisiones")
                .MapColumn(nameof(Invoice.Identificador), "Factura", 100)
                .MapColumn(nameof(Invoice.YearPeriod), "Período de facturación", 100)
                .MapColumn(nameof(Invoice.MonthPeriod), "Período de facturación", 100)
                .MapColumn(nameof(Invoice.Total), "Total", 100);

            return View(dataTableDefinition);
        }

        [HttpPost]
        public JsonResult Index(int index = 0, int length = 10, string orderProperty = null, bool inverse = false, [FromBody]IEnumerable<QueryFilter> query = null)
        {
            IQueryable<Invoice> invoice = _context.Invoice
                .Where(x => x.Comercial.UserName == User.Identity.Name && x.State > InvoiceStateEnum.Provisional);

            DataTableDefinition.DataQuery data =
                DataTableDefinition.DataQueryBuilder(index, length, orderProperty, inverse, query, invoice);

            return Json(data);
        }
    }
}