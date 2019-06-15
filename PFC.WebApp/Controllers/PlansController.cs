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
    [Authorize(Roles = "SuperAdmin,Gestor")]
    public class PlansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlansController(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            ViewBag.Title = "Tarifas";
        }

        // GET: Plans
        public async Task<IActionResult> Index()
        {
            var dataTableDefinition = DataTableDefinition.Create<Plan>()
                .WithTitle("Tarifas")
                .AddGlobalActions("Nuevo", Url.Action(nameof(Create)))
                .AddElementAction("Detalles", new DataTableDefinition.UrlActionDTO()
                {
                    Action = "Details",
                    Values = new Dictionary<string, string> { { "id", "ID" } }
                })
                .AddElementAction("Editar", new DataTableDefinition.UrlActionDTO()
                {
                    Action = "Edit",
                    Values = new Dictionary<string, string> { { "id", "ID" } }
                })
                .AddElementAction("Borrar", new DataTableDefinition.UrlActionDTO()
                {
                    Action = "Delete",
                    Values = new Dictionary<string, string> { { "id", "ID" } }
                })
                .MapColumn(nameof(Plan.Name), "Plan", 100)
                .MapColumn(nameof(Plan.VoicePlan), "Bono de voz", 100)
                .MapColumn(nameof(Plan.DataPlan), "Bono de datos", 100)
                .MapColumn(nameof(Plan.SMSPlan), "Bono de sms", 100)
                .MapColumn(nameof(Plan.Charge), "Coste", 100);

            return View(dataTableDefinition);
        }

        [HttpPost]
        public JsonResult Index(int index = 0, int length = 10, string orderProperty = null, bool inverse = false, [FromBody]IEnumerable<QueryFilter> query = null)
        {
            DataTableDefinition.DataQuery data =
                DataTableDefinition.DataQueryBuilder(index, length, orderProperty, inverse, query, _context.Plan);

            return base.Json(data, new Newtonsoft.Json.JsonSerializerSettings());
        }

        // GET: Plans/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var plan = await _context.Plan
                .SingleOrDefaultAsync(m => m.ID == id);

            if (plan == null)
                return NotFound();

            return View(plan);
        }

        // GET: Plans/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Plans/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Charge,VoicePlan,DataPlan,SMSPlan")] Plan plan)
        {
            if (!ModelState.IsValid)
                return View(plan);

            _context.Add(plan);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Plans/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var plan = await _context.Plan
                .SingleOrDefaultAsync(m => m.ID == id);

            if (plan == null)
                return NotFound();

            return View(plan);
        }

        // POST: Plans/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("ID,Name,Charge,VoicePlan,DataPlan,SMSPlan")] Plan plan)
        {
            if (!ModelState.IsValid)
                return View(plan);

            if (!_context.Plan.Any(e => e.ID == plan.ID))
                return NotFound();

            _context.Update(plan);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Plans/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var plan = await _context.Plan
                .SingleOrDefaultAsync(m => m.ID == id);

            if (plan == null)
                return NotFound();

            return View(plan);
        }

        // POST: Plans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var plan = await _context.Plan
                .SingleOrDefaultAsync(m => m.ID == id);

            if (plan == null)
                return NotFound();

            try
            {
                _context.Plan.Remove(plan);
                await _context.SaveChangesAsync();
            }
            catch
            {
                ModelState.AddModelError("", "No se ha podido borrar el plan, compruebe que no esté asignado a ningún abonado");
                return View(plan);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
