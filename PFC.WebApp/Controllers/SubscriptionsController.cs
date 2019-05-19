using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PFC.WebApp.Data;
using PFC.WebApp.Data.Models;
using PFC.WebApp.Models;
using PFC.WebApp.Models.InvoicesViewModel;
using PFC.WebApp.Support;
using PFC.WebApp.Support.PredicateBuilder;

namespace PFC.WebApp.Controllers
{
    public class SubscriptionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubscriptionsController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            ViewBag.Title = "Abonados";
        }

        // GET: Subscriptions
        public async Task<IActionResult> Index()
        {
            var dataTableDefinition = DataTableDefinition.Create<Subscription>()
                .WithTitle("Abonados")
                .AddGlobalActions("Nuevo", Url.Action(nameof(Create)), "Gestor,SuperAdmin")
                .AddElementAction("Detalles", new DataTableDefinition.UrlActionDTO()
                {
                    Action = "Details",
                    Values = new Dictionary<string, string> { { "id", "ID" } }
                })
                .AddElementAction("Editar", new DataTableDefinition.UrlActionDTO()
                {
                    Action = "Edit",
                    Values = new Dictionary<string, string> { { "id", "ID" } }
                }, "Gestor,SuperAdmin")
                .AddElementAction("Borrar", new DataTableDefinition.UrlActionDTO()
                {
                    Action = "Delete",
                    Values = new Dictionary<string, string> { { "id", "ID" } }
                }, "Gestor,SuperAdmin")
                .MapColumn(nameof(Subscription.UserName), "Usuario", 100)
                .MapColumn(nameof(Subscription.MSISDN), "Número de teléfono", 100)
                .MapColumn(nameof(Subscription.PlanName), "Tarifa", 100)
                .MapColumn(nameof(Subscription.SubscriptionDate), "Fecha de alta", 100)
                .MapColumn(nameof(Subscription.CancellationDate), "Fecha de baja", 100);

            return View(dataTableDefinition);
        }

        [HttpPost]
        public async Task<JsonResult> Index(int index = 0, int length = 10, string orderProperty = null, bool inverse = false, [FromBody]IEnumerable<QueryFilter> query = null)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            DataTableDefinition.DataQuery data =
                DataTableDefinition.DataQueryBuilder(index, length, orderProperty, inverse, query, _context.Subscriptions
                .Include(x => x.User)
                .Include(x => x.Plan)
                .Where(x => User.IsInRole("SuperAdmin") || User.IsInRole("Gestor") || x.UserId == user.Id));

            return base.Json(data, new Newtonsoft.Json.JsonSerializerSettings());
        }

        // GET: Subscriptions/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .Include(s => s.User)
                .SingleOrDefaultAsync(s => s.ID == id && (User.IsInRole("SuperAdmin") || User.IsInRole("Gestor") || s.User.UserName == User.Identity.Name));

            if (subscription == null)
                return NotFound();

            ViewBag.InvoiceDataTableDefinition = DataTableDefinition.Create<Invoice>()
                .WithTitle("Facturas")
                .AddGlobalActions("Nuevo", Url.Action(nameof(Invoice), new { id }), "Gestor,SuperAdmin")
                .AddElementAction("Detalles", new DataTableDefinition.UrlActionDTO()
                {
                    Action = "Details",
                    Controller = "Invoices",
                    Values = new Dictionary<string, string> { { "id", "ID" } }
                })
                .MapColumn(nameof(Data.Models.Invoice.Identificador), "Factura", 100)
                .MapColumn(nameof(Data.Models.Invoice.YearPeriod), "Año", 100)
                .MapColumn(nameof(Data.Models.Invoice.MonthPeriod), "Mes", 100)
                .MapColumn(nameof(Data.Models.Invoice.StateString), "Estado", 100)
                .MapColumn(nameof(Data.Models.Invoice.Total), "Total", 100);

            return View(subscription);
        }

        // GET: Subscriptions/Details/5
        [HttpPost]
        public async Task<IActionResult> Details(int id, int index = 0, int length = 10, string orderProperty = null, bool inverse = false, [FromBody]IEnumerable<QueryFilter> query = null)
        {
            IQueryable<Invoice> invoices = _context.Invoice;

            if (orderProperty == null)
                invoices = invoices.OrderByDescending(x => x.StartDate);

            DataTableDefinition.DataQuery data =
                DataTableDefinition.DataQueryBuilder(index, length, orderProperty, inverse, query, invoices.Where(x => x.SubscriptionId == id));

            return base.Json(data, new Newtonsoft.Json.JsonSerializerSettings());
        }

        // GET: Subscriptions/Create
        [Authorize(Roles = "Gestor,SuperAdmin")]
        public IActionResult Create()
        {
            ViewData["PlanId"] = new SelectList(_context.Plan, nameof(Plan.ID), nameof(Plan.Name));
            return View();
        }

        // POST: Subscriptions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Gestor,SuperAdmin")]
        public async Task<IActionResult> Create([Bind("ID,MSISDN,PlanId,SubscriptionDate,CancellationDate,UserId")] Subscription subscription)
        {
            //#error Validad fechas
            ValidateModel(subscription);

            ViewData["PlanId"] = new SelectList(_context.Plan, nameof(Plan.ID), nameof(Plan.Name));
            if (!ModelState.IsValid)
                return View(subscription);

            _context.Add(subscription);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Subscriptions/Edit/5
        [Authorize(Roles = "Gestor,SuperAdmin")]
        public async Task<IActionResult> Edit(int id)
        {
            var subscription = await _context.Subscriptions
                .Include(x => x.User)
                .SingleOrDefaultAsync(m => m.ID == id);

            if (subscription == null)
                return NotFound();

            ViewData["PlanId"] = new SelectList(_context.Plan, nameof(Plan.ID), nameof(Plan.Name));

            if (nonEditableSubscription(subscription))
                this.DisallowFormEdition("Existe algun abonado con fecha de alta posterior al actual");

            return View(subscription);
        }

        // POST: Subscriptions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Gestor,SuperAdmin")]
        public async Task<IActionResult> Edit([Bind("ID,UserId,MSISDN,PlanId,SubscriptionDate,CancellationDate")] Subscription subscription)
        {
            if (!_context.Subscriptions.Any(x => x.ID == subscription.ID))
                return NotFound();

            ValidateModel(subscription);

            // intento de edición del formulario no autorizado, debería haberse deshabilitado en el get
            if (ModelState.IsValid && nonEditableSubscription(subscription))
                return Unauthorized();


            ViewData["PlanId"] = new SelectList(_context.Plan, nameof(Plan.ID), nameof(Plan.Name));
            if (!ModelState.IsValid)
                return View(subscription);

            var entry = _context.Update(subscription);

            // Ignoramos la actualización del usuario
            entry.Property(x => x.UserId).IsModified = false;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool nonEditableSubscription(Subscription subscription)
        {
            return _context.Subscriptions.Any(x => x.MSISDN == subscription.MSISDN && x.SubscriptionDate > subscription.CancellationDate);
        }

        private void ValidateModel(Subscription subscription)
        {
            // Validación del rango de fechas de alta del abonado
            if (subscription.CancellationDate < subscription.SubscriptionDate)
                ModelState.AddModelError(nameof(Subscription.CancellationDate), "La fecha de baja no puede ser menor a la fecha de alta");

            var otherSubscriptions = _context.Subscriptions
                .Where(x => x.MSISDN == subscription.MSISDN && x.ID != subscription.ID);

            Subscription lastActiveSubscription = otherSubscriptions.Where(x => !x.CancellationDate.HasValue || x.CancellationDate >= subscription.SubscriptionDate).LastOrDefault();
            if (lastActiveSubscription != null)
            {
                if (lastActiveSubscription.CancellationDate.HasValue)
                    ModelState.AddModelError(nameof(Subscription.SubscriptionDate), $"La fecha de alta del abonado debe ser mayor que {lastActiveSubscription.CancellationDate.Value.ToShortDateString()}");
                else
                    ModelState.AddModelError("", $"Existe algún abonado para esta línea que aún no ha sido dado de baja");
            }
        }

        // GET: Subscriptions/Delete/5
        [Authorize(Roles = "Gestor,SuperAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .SingleOrDefaultAsync(m => m.ID == id);

            if (subscription == null)
                return NotFound();

            if (nonEditableSubscription(subscription))
                this.DisallowFormEdition("Existe algun abonado con fecha de alta posterior al actual");

            return View(subscription);
        }

        // POST: Subscriptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Gestor,SuperAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subscription = await _context.Subscriptions
                .SingleOrDefaultAsync(m => m.ID == id);

            if (subscription == null)
                return NotFound();

            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Gestor,SuperAdmin")]
        public async Task<IActionResult> Invoice(int id)
        {
            // Calculamos el período de facturación que tenga pendiente
            var subscription = _context.Subscriptions
                    .Where(x => x.ID == id)
                    .Include(x => x.Invoices)
                    .SingleOrDefault();

            if (subscription == null)
                return NotFound();

            GenerateInvoiceViewBag(subscription);

            return View(new InvoiceViewModel() { SubscriptionId = id });
        }

        [HttpPost]
        [Authorize(Roles = "Gestor,SuperAdmin")]
        public async Task<IActionResult> Invoice(InvoiceViewModel invoiceViewModel)
        {
            var subscription = _context.Subscriptions
                .Include(x => x.Plan)
                .Include(x => x.Invoices)
                .Where(x => x.ID == invoiceViewModel.SubscriptionId)
                .SingleOrDefault();

            if (subscription == null)
                return NotFound();

            // Nos aseguramos que la fecha seleccionada para el período siempre es el dia primero de cada mes
            invoiceViewModel.Period = new DateTime(invoiceViewModel.Period.Year, invoiceViewModel.Period.Month, 1);

            if (subscription.Invoices.Any(x => x.SubscriptionId == subscription.ID && x.StartDate.Year == invoiceViewModel.Period.Year && x.StartDate.Month == invoiceViewModel.Period.Month))
            {
                ModelState.AddModelError("", "Ya existe una factura para este abonado en el mismo período");
                GenerateInvoiceViewBag(subscription);
                return View(invoiceViewModel);
            }

            var invoice = _context.PhoneEvents
                .Where(x => x.SourceId == subscription.MSISDN)
                // Filtro de llamadas que pertenecen al período de vigencia del abonado
                .Where(x => x.Date.Date >= subscription.SubscriptionDate.Date && (!subscription.CancellationDate.HasValue || x.Date.Date <= subscription.CancellationDate.Value.Date))
                // Filtro de llamadas pertenecientes al período de facturación seleccionado
                .Where(x => x.Date.Year == invoiceViewModel.Period.Year && x.Date.Month == invoiceViewModel.Period.Month)
                .GroupBy(x => x.SourceId)
                .Select(pe => new Invoice()
                {
                    CallDuration = pe.Sum(x => x.Type == PhoneEventType.Voz ? x.Duration : 0),
                    CallPrice = pe.Sum(x => x.Type == PhoneEventType.Voz ? x.Charge : 0),
                    DataTrafficBytes = pe.Sum(x => x.Type == PhoneEventType.Datos ? x.Duration : 0),
                    DataTrafficPrice = pe.Sum(x => x.Type == PhoneEventType.Datos ? x.Charge : 0),
                    SMS = pe.Count(x => x.Type == PhoneEventType.SMS),
                    SMSPrice = pe.Sum(x => x.Type == PhoneEventType.SMS ? x.Charge : 0),
                    PhoneEvents = pe
                }).SingleOrDefault() ?? new Invoice() { PhoneEvents = new List<PhoneEvent>() };

            invoice.SubscriptionId = subscription.ID;

            invoice.StartDate = invoiceViewModel.Period.Date > subscription.SubscriptionDate ? new DateTime(invoiceViewModel.Period.Year, invoiceViewModel.Period.Month, 1) : subscription.SubscriptionDate.Date;
            invoice.EndDate = subscription.CancellationDate.HasValue && subscription.CancellationDate.Value < invoiceViewModel.Period.Date ? subscription.CancellationDate.Value : invoiceViewModel.Period.Date.AddMonths(1).AddDays(-1);

            invoice.PlanPrice = subscription.Plan.Charge;
            invoice.Subtotal = invoice.PlanPrice + invoice.CallPrice + invoice.DataTrafficPrice + invoice.SMSPrice - invoice.CallPriceDeduction - invoice.DataTrafficPriceDeduction - invoice.SMSPriceDeduction;
            invoice.TaxRate = 0.07m;
            invoice.Total = invoice.Subtotal + invoice.Subtotal * invoice.TaxRate;

            _context.Invoice.Add(invoice);
            _context.SaveChanges();

            // Establecemos el identificador de la factura en cada uno de los eventos telefónicos
            foreach (var pe in invoice.PhoneEvents)
                pe.InvoiceId = invoice.ID;

            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = invoiceViewModel.SubscriptionId });
        }

        private void GenerateInvoiceViewBag(Subscription subscription)
        {
            var pendingInvoicesStartDate = subscription.SubscriptionDate;
            var pendingInvoicesEndDate = subscription.CancellationDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, 1);

            IEnumerable<KeyValuePair<DateTime, string>> items = getPendingInvoicesPeriod(pendingInvoicesStartDate, pendingInvoicesEndDate)
                .Where(x => !subscription.Invoices.Any(s => s.StartDate.Month == x.Key.Month && s.StartDate.Year == x.Key.Year))
                .ToList();

            if (items.Count() == 0)
                this.DisallowFormEdition("No hay facturas pendientes de emisión para el abonado actual");

            ViewBag.Periodos = new SelectList(items, "Key", "Value");
        }

        private static IEnumerable<KeyValuePair<DateTime, string>> getPendingInvoicesPeriod(DateTime pendingInvoicesStartDate, DateTime pendingInvoicesEndDate)
        {
            for (var i = pendingInvoicesStartDate; i <= pendingInvoicesEndDate; i = i.AddMonths(1))
                yield return new KeyValuePair<DateTime, string>(new DateTime(i.Year, i.Month, 1), i.ToString("MMMM yyy"));
        }
    }
}
