using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PFC.WebApp.Data;
using PFC.WebApp.Data.Models;
using PFC.WebApp.Models;
using PFC.WebApp.Models.MasMovilViewModels;
using PFC.WebApp.Services.DocumentManager;
using PFC.WebApp.Support;
using PFC.WebApp.Support.FilesSupport;
using PFC.WebApp.Support.PredicateBuilder;

namespace PFC.WebApp.Controllers
{
    [Authorize(Roles = "SuperAdmin,Gestor")]
    public class MasMovilCSVController : Controller
    {
        private readonly IDocumentRepository documentRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly IContentTypeProvider _contentTypeProvider;
        private readonly ApplicationDbContext _applicationDbContext;

        public MasMovilCSVController(IServiceProvider serviceProvider,
            IDocumentRepositoryManager documentRepositoryManager,
            IContentTypeProvider contentTypeProvider,
            ApplicationDbContext applicationDbContext)
        {
            documentRepository = documentRepositoryManager.Create("MasMovilCSV");
            _serviceProvider = serviceProvider;
            _contentTypeProvider = contentTypeProvider;
            _applicationDbContext = applicationDbContext;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            ViewBag.Title = "MásMóvil CSV";
        }

        // GET: DocumentManager
        public ActionResult Index()
        {
            var dataTableDefinition = new DataTableDefinition(_applicationDbContext.MasMovilFileStates)
                .WithTitle("Ficheros")
                .WithListAction(Url.Action("Index"))
                .MapColumn("Name", "Nombre", 80)
                .MapColumn("StateString", "Estado", 20)
                .AddGlobalActions("Nuevo", Url.Action("Create"))
                .AddElementAction("Detalles", new DataTableDefinition.UrlActionDTO()
                {
                    Action = "Details",
                    Values = new Dictionary<string, string> { { "id", "Name" } }
                })
                .AddElementAction("Importar", new DataTableDefinition.UrlActionDTO()
                {
                    Action = "Import",
                    Values = new Dictionary<string, string> { { "id", "Name" } }
                })
                .AddElementAction("Descargar", new DataTableDefinition.UrlActionDTO()
                {
                    Action = "Download",
                    Values = new Dictionary<string, string> { { "id", "Name" } }
                })
                .AddElementAction("Borrar", new DataTableDefinition.UrlActionDTO()
                {
                    Action = "Delete",
                    Values = new Dictionary<string, string> { { "id", "Name" } }
                });

            return View(nameof(Index), dataTableDefinition);
        }

        [HttpPost]
        public JsonResult Index(int index = 0, int length = 10, string orderProperty = null, bool inverse = false, [FromBody]IEnumerable<QueryFilter> query = null)
        {
            DataTableDefinition.DataQuery data =
                DataTableDefinition.DataQueryBuilder(index, length, orderProperty, inverse, query, _applicationDbContext.MasMovilFileStates);
            return base.Json(data, new Newtonsoft.Json.JsonSerializerSettings()
            {
                Culture = System.Threading.Thread.CurrentThread.CurrentUICulture
            });
        }

#warning Cache provisional para mantener el fichero en memoria cargado (implementar una cache por session de usuario y )
        private static ConcurrentDictionary<string, IList<MasMovilEDR>> cache = new ConcurrentDictionary<string, IList<MasMovilEDR>>();
        private bool cacheEnable = true;

        // GET: DocumentManager/Details/5
        public ActionResult Details(string id)
        {
            var documentFileState = _applicationDbContext.MasMovilFileStates
                .SingleOrDefault(x => x.Name == id);

            if (documentFileState == null)
                return NotFound();

            if (cacheEnable)
            {
                IDocument document = documentRepository.GetDocument(id);
                Task.Run(() =>
                {
                    if (!cache.ContainsKey(id))
                    {
                        lock (cache)
                        {
                            if (!cache.ContainsKey(id))
                            {
                                using (var csvReader = new CSVReader(document.GetReadStream(), '|'))
                                {

                                    cache[id] = csvReader.Select(x => MasMovilEDR.Parse(x)).ToList();
                                }
                            }
                        }
                    }
                });
            }

            ViewBag.MasMovilFileDataTableDefinition = new DataTableDefinition(typeof(MasMovilEDR))
                .WithTitle(id)
                .AutoMapColumn();

            return View(documentFileState);
        }

        [HttpPost]
        public JsonResult Details(string id, int index = 0, int length = 10, string orderProperty = null, bool inverse = false, [FromBody]IEnumerable<QueryFilter> query = null)
        {
            lock (cache)
            {
                if (cacheEnable && cache.ContainsKey(id))
                    return Json(DataTableDefinition.DataQueryBuilder(index, length, orderProperty, inverse, query, cache[id].AsQueryable(), cacheEnable));
            }

            using (var csvReader = new CSVReader(documentRepository.GetDocument(id).GetReadStream(), '|'))
            {
                var queryable = csvReader.Select(x => MasMovilEDR.Parse(x)).AsQueryable();
                return Json(DataTableDefinition.DataQueryBuilder(index, length, orderProperty, inverse, query, queryable, cacheEnable));
            }
        }

        // GET: DocumentManager/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DocumentManager/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile File)
        {
            try
            {
                if (File == null)
                {
                    ModelState.AddModelError("", $"No se adjuntó ningún fichero en el formulario");
                    return View();
                }

                if (_applicationDbContext.MasMovilFileStates.Any(x => x.Name == File.FileName))
                {
                    ModelState.AddModelError("", $"Ya existe un fichero con el mismo nombre");
                    return View();
                }

                int line = 0;

                await Task.Run(() =>
                {
                    var document = documentRepository.CreateDocument(File.FileName, File.OpenReadStream());

                    using (var csvReader = new CSVReader(document.GetReadStream(), '|'))
                    {
                        foreach (var item in csvReader)
                        {
                            line++;
                            if (!MasMovilEDR.TryParse(item, out _, out string error))
                                ModelState.AddModelError("", $"Ocurrió un error al parsear la línea {line} del fichero CSV. {error}. Línea : {string.Join("|", item)}");
                        }
                    }

                    if (!ModelState.IsValid)
                        documentRepository.DeleteDocument(document.Name);
                });

                // Si el fichero no es correcto se muestran los mensajes de validación al usuario y se borra el fichero
                if (!ModelState.IsValid)
                    return View();

                // Se establece el estado de importación del fichero
                _applicationDbContext.MasMovilFileStates
                    .Add(new MasMovilFileState()
                    {
                        Name = File.FileName,
                        RegistersCount = line,
                        State = MasMovilFileStateEnum.Nuevo
                    });

                await _applicationDbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));

            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        // GET: DocumentManager/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            var fileState = await _applicationDbContext.MasMovilFileStates
                    .SingleOrDefaultAsync(x => x.Name == id);

            if (fileState == null)
                return NotFound();

            if (fileState.State != MasMovilFileStateEnum.Nuevo)
                ModelState.AddModelError("", "El fichero no se puede borrar porque ha sido importado a la base de datos");

            return View(fileState);
        }

        // POST: DocumentManager/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id, IFormCollection collection)
        {
            var fileState = _applicationDbContext.MasMovilFileStates
                .SingleOrDefault(x => x.Name == id);

            if (fileState == null)
                return NotFound();

            if (fileState.State != MasMovilFileStateEnum.Nuevo && fileState.State != MasMovilFileStateEnum.Error)
            {
                ModelState.AddModelError("", "El fichero no se puede borrar porque ha sido importado a la base de datos");
                return View();
            }

            documentRepository.DeleteDocument(id);
            _applicationDbContext.MasMovilFileStates.Remove(fileState);
            await _applicationDbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Download(string id)
        {
            var document = documentRepository.GetDocument(id);

            if (!_contentTypeProvider.TryGetContentType(document.Name, out string contentType))
                contentType = "application/octet-stream";

            return File(document.GetReadStream(), contentType, document.Name);
        }

        ConcurrentDictionary<string, Task> importaciones = new ConcurrentDictionary<string, Task>();

        public async Task<IActionResult> Import(string id)
        {
            var fileState = await _applicationDbContext.MasMovilFileStates.SingleOrDefaultAsync(x => x.Name == id);

            if (fileState.State == MasMovilFileStateEnum.Importado)
            {
                ModelState.AddModelError("", "El fichero ya ha sido importado");
                return Index();
            }

            if (fileState.State == MasMovilFileStateEnum.Importando && importaciones.ContainsKey(id))
            {
                ModelState.AddModelError("", "El fichero se está importando");
                return Index();
            }

            var task = GetTask(id);

            if (importaciones.TryAdd(id, task))
            {
                fileState.State = MasMovilFileStateEnum.Importando;
                _applicationDbContext.SaveChanges();

                task.Start();
            }

            return RedirectToAction(nameof(Index));
        }

        private Task GetTask(string id)
        {
            return new Task(() =>
            {
                try
                {
                    using (var events = new CSVReader(documentRepository.GetDocument(id).GetReadStream(), '|'))
                    {
                        IEnumerable<PhoneEvent> entities = events
                            .Select((x, i) => MasMovilEDR.Parse(x).ToPhoneEvent($"{id}:{i}"));

                        using (IServiceScope serviceScope = _serviceProvider.CreateScope())
                        {
                            using (var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>())
                            {
                                context.PhoneEvents.AddRange(entities);
                                var fileState = context.MasMovilFileStates.SingleOrDefault(x => x.Name == id);
                                fileState.State = MasMovilFileStateEnum.Importado;
                                context.SaveChanges();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    using (IServiceScope serviceScope = _serviceProvider.CreateScope())
                    {
                        using (var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>())
                        {
                            var fileState = context.MasMovilFileStates.SingleOrDefault(x => x.Name == id);
                            fileState.State = MasMovilFileStateEnum.Error;
                            fileState.Error = ex.Message;
                            context.SaveChanges();
                        }
                    }
                }
                finally
                {
                    importaciones.TryRemove(id, out _);
                }
            });
        }
    }
}