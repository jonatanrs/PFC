using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PFC.WebApp.Models;
using PFC.WebApp.Models.AccountViewModels;
using PFC.WebApp.Models.AdminUsersViewModels;
using PFC.WebApp.Services;
using PFC.WebApp.Support;
using PFC.WebApp.Support.PredicateBuilder;


namespace PFC.WebApp.Controllers
{

    [Authorize(Roles = "SuperAdmin, Gestor")]
    public class AdminUsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AdminUsersController> _logger;

        public AdminUsersController(
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            ILogger<AdminUsersController> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            ViewBag.Title = "Administración de usuarios";
        }

        [HttpGet]
        public IActionResult Index()
        {
            var dataTableDefinition = DataTableDefinition.Create<AdminUserViewModel>()
                .WithTitle("Usuarios")
                .AddGlobalActions("Nuevo", Url.Action(nameof(Create)))
                .AddElementAction("Editar", new DataTableDefinition.UrlActionDTO() {
                    Action = "Edit",
                    Values = new Dictionary<string, string> { { "id", "ID" } }
                })
                .AddElementAction("(Des) Asignar Gestor", new DataTableDefinition.UrlActionDTO()
                {
                    Action = "ToggleGestorRole",
                    Values = new Dictionary<string, string> { { "id", "ID" } }
                })
                .AddElementAction("(Des) Asignar Comercial", new DataTableDefinition.UrlActionDTO()
                {
                    Action = "ToggleComercialRole",
                    Values = new Dictionary<string, string> { { "id", "ID" } }
                })
                .AddElementAction("(Des) Bloquear", new DataTableDefinition.UrlActionDTO()
                {
                    Action = "ToggleLock",
                    Values = new Dictionary<string, string> { { "id", "ID" } }
                })
                .AddElementAction("Borrar", new DataTableDefinition.UrlActionDTO()
                {
                    Action = "Delete",
                    Values = new Dictionary<string, string> { { "id", "ID" } }
                })
                .MapColumn(nameof(AdminUserViewModel.Usuario), "Nombre", 100)
                .MapColumn(nameof(AdminUserViewModel.Roles), "Roles", 100)
                .MapColumn(nameof(AdminUserViewModel.Comercial), "Comercial", 100)
                .MapColumn(nameof(AdminUserViewModel.IsLocked), "Bloqueado", 100);

            return View(dataTableDefinition);
        }

        [HttpPost]
        public async Task<IActionResult> Index(int index = 0, int length = 10, string orderProperty = null, bool inverse = false, [FromBody]IEnumerable<QueryFilter> query = null)
        {
            var superAdminUserIDs = (await _userManager.GetUsersInRoleAsync("SuperAdmin")).Select(x => x.Id);
            var usuarios = _userManager.Users
                .Include(x => x.Comercial)
                .Where(x => !superAdminUserIDs.Contains(x.Id))
                .ToList();

            var queryable = usuarios.Select(x => new AdminUserViewModel()
            {
                ID = x.Id,
                Usuario = x.UserName,
                Roles = string.Join(" | ", _userManager.GetRolesAsync(x).Result),
                Comercial = x.Comercial?.UserName ?? String.Empty,
                IsLocked = _userManager.IsLockedOutAsync(x).Result

            }).AsQueryable();

            DataTableDefinition.DataQuery data =
                DataTableDefinition.DataQueryBuilder(index, length, orderProperty, inverse, query, queryable);

            return base.Json(data, new Newtonsoft.Json.JsonSerializerSettings());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.NIF,
                    Email = model.Email,
                    ComercialId = model.ComercialId
                };

                var result = await _userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                    await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                    _logger.LogInformation($"Nuevo cliente creado NIF: {model.NIF} Email: {model.Email}.");

                    return RedirectToAction(nameof(Index));
                }

                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);

            if (user == null)
                return NotFound();

            return base.View(new RegisterViewModel() {
                NIF = user.UserName,
                Email = user.Email,
                ComercialId = user.ComercialId
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string Id, RegisterViewModel model)
        {
            var user = await _userManager.FindByIdAsync(Id);

            if (user == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                if (user.Email != model.Email)
                    user.EmailConfirmed = false;

                user.Email = model.Email;
                user.ComercialId = model.ComercialId;
                user.UserName = model.NIF;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Cliente {Id} actualizado, NIF: {model.NIF} Email: {model.Email} Comercial: {model.ComercialId}.");

                    if (!user.EmailConfirmed)
                    {
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                        await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                        _logger.LogInformation($"Correo de confirmación de correo electrónico enviado al cliente NIF: {model.NIF} Email: {model.Email}.");
                    }
                    return RedirectToAction(nameof(Index));
                }

                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> ToggleGestorRole(string id)
        {
            await toggleRole(id, "Gestor");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ToggleComercialRole(string id)
        {
            await toggleRole(id, "Comercial");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ToggleLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var isLocked = await _userManager.IsLockedOutAsync(user);
            await _userManager.SetLockoutEndDateAsync(user, isLocked ? (DateTime?)null : DateTime.MaxValue);

            return RedirectToAction(nameof(Index));
        }

        private async Task toggleRole(string id, string Role)
        {
            var user = await _userManager.FindByIdAsync(id);
            bool isInRole = await _userManager.IsInRoleAsync(user, Role);

            await (isInRole
                ? _userManager.RemoveFromRoleAsync(user, Role)
                : _userManager.AddToRoleAsync(user, Role));
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}