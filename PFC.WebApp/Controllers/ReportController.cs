using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using PFC.WebApp.Data;
using PFC.WebApp.Models.Report;

namespace PFC.WebApp.Controllers
{
    [Authorize(Roles = "SuperAdmin,Gestor")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            ViewBag.Title = "Informes";
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> PhoneEventsWithoutSubscription()
        {
            var result = await _context.PhoneEvents
                .GroupJoin(_context.Subscriptions,
                    pe => pe.SourceId,
                    s => s.MSISDN,
                    (pe, s) => new
                    {
                        PhoneEvent = pe,
                        Subscription = s.Where(x => pe.Date >= x.SubscriptionDate && (!x.CancellationDate.HasValue || pe.Date < x.CancellationDate.Value.AddDays(1)))
                    })
                    .Where(x => !x.Subscription.Any())
                    .Select(x => x.PhoneEvent)
                    .GroupBy(x => x.SourceId)
                    .Select(x => new PhoneEventsWithoutSubscriptionDTO
                    {
                        MSISDN = x.Key,
                        EventsCount = x.Count(),
                        First = x.Select(pe => pe.Date).Min(),
                        Last = x.Select(pe => pe.Date).Max()
                    })
                    .Distinct()
                    .ToListAsync();

            return View(result);
        }

        public async Task<IActionResult> SubscriptionsWithPendingInvoices()
        {
            var result = await _context.Subscriptions
                .Where(x => x.Invoices.Count < ((x.CancellationDate ?? DateTime.Now).Year - x.SubscriptionDate.Year) * 12 + (x.CancellationDate ?? DateTime.Now).Month - x.SubscriptionDate.Month)
                .Include(x => x.Invoices)
                .ToListAsync();

            return View(result);
        }
    }
}