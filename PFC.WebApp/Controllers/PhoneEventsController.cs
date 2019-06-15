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
    [Authorize(Roles= "SuperAdmin,Gestor")]
    public class PhoneEventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PhoneEventsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            ViewBag.Title = "Eventos telefónicos";
        }

        public ActionResult Index()
        {
            var dataTableDefinition = new DataTableDefinition(_context.PhoneEvents)
                .WithTitle("Eventos telefónicos")
                .WithListAction(Url.Action("Index"))
                .MapColumn("Date", "Fecha", 20, "complex")
                .MapColumn("SourceId", "Origen", 20)
                .MapColumn("DestinationId", "Destino", 20)
                .MapColumn("FriendlyDuration", "Duración", 20)
                .MapColumn("TypeString", "Tipo", 20)
                .MapColumn("Charge", "Coste", 20)
                .AddElementAction("Detalles", new DataTableDefinition.UrlActionDTO()
                {
                    Action = "Details",
                    Values = new Dictionary<string, string> { { "id", "ID" } }
                }).AddElementAction("Origen", new DataTableDefinition.UrlActionDTO()
                {
                    Action = "Origin",
                    Values = new Dictionary<string, string> { { "id", "ID" } }
                });

            return View(dataTableDefinition);
        }

        [HttpPost]
        // GET: PhoneEvents
        public JsonResult Index(int index = 0, int length = 10, string orderProperty = null, bool inverse = false, [FromBody]IEnumerable<QueryFilter> query = null)
        {
            DataTableDefinition.DataQuery data =
                DataTableDefinition.DataQueryBuilder(index, length, orderProperty, inverse, query, _context.PhoneEvents);
            return base.Json(data);
        }

        // GET: PhoneEvents/Edit/5
        public async Task<IActionResult> Details(long id)
        {
            var phoneEvent = await _context.PhoneEvents
                .SingleOrDefaultAsync(m => m.ID == id);

            if (phoneEvent == null)
                return NotFound();

            return View(phoneEvent);
        }

        public async Task<IActionResult> Origin(long id)
        {
            var phoneEvent = await _context.PhoneEvents
                .SingleOrDefaultAsync(m => m.ID == id);

            if (phoneEvent == null)
                return NotFound();

            switch (phoneEvent.Provider)
            {
                case "MasMovilProvider":
                    return RedirectToAction(nameof(MasMovilCSVController.Details), nameof(MasMovilCSVController).Replace("Controller", ""), new { id = phoneEvent.Localizer.Split(":")[0] });
                default:
                    return NotFound();
            }
        }
    }
}
