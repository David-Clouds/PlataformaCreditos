using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using PlataformaCreditos.Data;
using PlataformaCreditos.Models;

namespace PlataformaCreditos.Controllers
{
    [Authorize]
    public class SolicitudesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IDistributedCache _cache;

        public SolicitudesController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IDistributedCache cache)
        {
            _context = context;
            _userManager = userManager;
            _cache = cache;
        }

        
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var cacheKey = $"solicitudes_{userId}";

            var cacheData = await _cache.GetStringAsync(cacheKey);

            if (cacheData != null)
            {
                var solicitudesCache = System.Text.Json.JsonSerializer
                    .Deserialize<List<SolicitudCredito>>(cacheData);

                return View(solicitudesCache);
            }

            var solicitudes = await _context.Solicitudes
                .Include(s => s.Cliente)
                .Where(s => s.Cliente.UsuarioId == userId)
                .ToListAsync();

            var data = System.Text.Json.JsonSerializer.Serialize(solicitudes);

            await _cache.SetStringAsync(cacheKey, data,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                });

            return View(solicitudes);
        }

        
        public async Task<IActionResult> Detalle(int id)
        {
            var solicitud = await _context.Solicitudes
                .Include(s => s.Cliente)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
                return NotFound();

            
            HttpContext.Session.SetString("ultima_solicitud",
                solicitud.MontoSolicitado.ToString());

            return View(solicitud);
        }

        public IActionResult Crear()
        {
            return View();
        }

      
        [HttpPost]
        public async Task<IActionResult> Crear(decimal monto)
        {
            var userId = _userManager.GetUserId(User);

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.UsuarioId == userId);

            if (cliente == null || !cliente.Activo)
            {
                TempData["Error"] = "Cliente no válido";
                return View();
            }

            var existe = _context.Solicitudes
                .Any(s => s.ClienteId == cliente.Id && s.Estado == "Pendiente");

            if (existe)
            {
                TempData["Error"] = "Ya tienes una solicitud pendiente";
                return View();
            }

            if (monto > cliente.IngresosMensuales * 10)
            {
                TempData["Error"] = "Monto excede el límite";
                return View();
            }

            var solicitud = new SolicitudCredito
            {
                ClienteId = cliente.Id,
                MontoSolicitado = monto,
                FechaSolicitud = DateTime.Now,
                Estado = "Pendiente"
            };

            _context.Solicitudes.Add(solicitud);
            await _context.SaveChangesAsync();

        
            var cacheKey = $"solicitudes_{userId}";
            await _cache.RemoveAsync(cacheKey);

            TempData["Ok"] = "Solicitud registrada";

            return RedirectToAction("Index");
        }
    }
}