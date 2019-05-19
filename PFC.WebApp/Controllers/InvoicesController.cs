using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PFC.WebApp.Data;
using PFC.WebApp.Data.Models;
using PFC.WebApp.Support;
using PFC.WebApp.Support.PredicateBuilder;

namespace PFC.WebApp.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvoicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            ViewBag.Title = "Facturas";
        }

        // GET: Invoices
        public async Task<IActionResult> Index()
        {
            var dataTableDefinition = DataTableDefinition.Create<Invoice>()
                .WithTitle("Facturas")
                .AddElementAction("Detalles", new DataTableDefinition.UrlActionDTO()
                {
                    Action = "Details",
                    Values = new Dictionary<string, string> { { "id", "ID" } }
                })
                .AddElementAction("Emitir", new DataTableDefinition.UrlActionDTO()
                {
                    Action = "Emit",
                    Values = new Dictionary<string, string> { { "id", "ID" } }
                }, "Gestor,SuperAdmin")
                .AddElementAction("Borrar", new DataTableDefinition.UrlActionDTO()
                {
                    Action = "Delete",
                    Values = new Dictionary<string, string> { { "id", "ID" } }
                }, "Gestor,SuperAdmin")
                .MapColumn(nameof(Invoice.Identificador), "Factura", 100)
                .MapColumn(nameof(Invoice.YearPeriod), "Período de facturación", 100)
                .MapColumn(nameof(Invoice.MonthPeriod), "Período de facturación", 100)
                .MapColumn(nameof(Invoice.StateString), "Estado", 100)
                .MapColumn(nameof(Invoice.Total), "Total", 100);

            return View(dataTableDefinition);
        }

        [HttpPost]
        public JsonResult Index(int index = 0, int length = 10, string orderProperty = null, bool inverse = false, [FromBody]IEnumerable<QueryFilter> query = null)
        {
            IQueryable<Invoice> invoice = _context.Invoice;
            if (!User.IsInRole("Gestor") && !User.IsInRole("SuperAdmin"))
                invoice = invoice.Where(x => x.Subscription.User.UserName == User.Identity.Name);

            DataTableDefinition.DataQuery data =
                DataTableDefinition.DataQueryBuilder(index, length, orderProperty, inverse, query, invoice);

            return Json(data);
        }

        // GET: Invoices/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _context.Invoice
                .Include(i => i.Subscription)
                .SingleOrDefaultAsync(m => m.ID == id);

            if (invoice == null)
                return NotFound();

            ViewBag.PhoneEventsDataTableDefinition = DataTableDefinition.Create<PhoneEvent>()
                .WithTitle("Eventos telefónicos")
                .WithListAction(Url.Action("InvoicePhoneEvents", new { id = id}))
                .MapColumn("Date", "Fecha", 20, "complex")
                .MapColumn("SourceId", "Origen", 20)
                .MapColumn("DestinationId", "Destino", 20)
                .MapColumn("FriendlyDuration", "Duración", 20)
                .MapColumn("TypeString", "Tipo", 20)
                .MapColumn("Charge", "Coste", 20);

            return View(invoice);
        }

        public JsonResult InvoicePhoneEvents(int id, int index = 0, int length = 10, string orderProperty = null, bool inverse = false, [FromBody]IEnumerable<QueryFilter> query = null)
        {
            var isGestor = User.IsInRole("Gestor") || User.IsInRole("SuperAdmin");

            var invoice = _context.Invoice
                .Where(x => x.ID == id && (isGestor || x.Subscription.User.UserName == User.Identity.Name))
                .SingleOrDefault();

            if (invoice == null)
                throw new UnauthorizedAccessException();
            

            DataTableDefinition.DataQuery data =
                DataTableDefinition.DataQueryBuilder(index, length, orderProperty, inverse, query, _context.PhoneEvents.Where(x => x.InvoiceId == invoice.ID));

            return Json(data);
        }

        // GET: Invoices/Delete/5
        [Authorize(Roles = "SuperAdmin, Gestor")]
        public async Task<IActionResult> Emit(int id)
        {
            var invoice = await _context.Invoice
                .Include(i => i.Subscription)
                .SingleOrDefaultAsync(m => m.ID == id);

            if (invoice == null)
                return NotFound();

            if (invoice.State != InvoiceStateEnum.Provisional)
                this.DisallowFormEdition("No se puede emitir la factura porque ya ha sido emitida.");

            return View(invoice);
        }

        // POST: Invoices/Delete/5
        [ValidateAntiForgeryToken]
        [HttpPost, ActionName("Emit")]
        [Authorize(Roles = "SuperAdmin, Gestor")]
        public async Task<IActionResult> EmitConfirmed(int id)
        {
            var invoice = await _context.Invoice
                .Include(x => x.Subscription.User)
                .SingleOrDefaultAsync(m => m.ID == id);

            if (invoice.State != InvoiceStateEnum.Provisional)
            {
                ModelState.AddModelError("", "La factura ya ha sido emitida, si desea volver a generar la factura bórrela y vuelva a generarla");
                return View(invoice);
            }

            invoice.State = InvoiceStateEnum.Final;
            invoice.EmissionDate = DateTime.Now;
            invoice.ComercialId = invoice.Subscription.User.ComercialId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // GET: Invoices/Delete/5
        [Authorize(Roles = "SuperAdmin, Gestor")]
        public async Task<IActionResult> Delete(int id)
        {
            var invoice = await _context.Invoice
                .Include(i => i.Subscription)
                .SingleOrDefaultAsync(m => m.ID == id);

            if (invoice == null)
                return NotFound();

            if (invoice.State != InvoiceStateEnum.Provisional)
                this.DisallowFormEdition("La factura no se puede borrar porque ya ha sido emitidade forma definitiva");

            return View(invoice);
        }

        // POST: Invoices/Delete/5
        [ValidateAntiForgeryToken]
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "SuperAdmin, Gestor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var invoice = await _context.Invoice
                .SingleOrDefaultAsync(m => m.ID == id);

            if (invoice.State != InvoiceStateEnum.Provisional)
            {
                ModelState.AddModelError("", "La factura no se puede borrar porque ya ha sido emitidade forma definitiva");
                return View(invoice);
            }

            _context.Invoice.Remove(invoice);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
