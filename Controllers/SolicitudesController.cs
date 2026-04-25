using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using PlataformaCreditos.Data;
using PlataformaCreditos.Models;

namespace PlataformaCreditos.Controllers
{
    [Authorize]
    public class SolicitudesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SolicitudesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

  
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.UsuarioId == userId);

            if (cliente == null)
                return RedirectToAction("Crear");

            var solicitudes = await _context.Solicitudes
                .Where(s => s.ClienteId == cliente.Id)
                .ToListAsync();

            return View(solicitudes);
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
                TempData["Error"] = "Monto excede el límite permitido";
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

            TempData["Ok"] = "Solicitud registrada correctamente";

            return RedirectToAction("Index");
        }

      
        public async Task<IActionResult> Detalle(int id)
        {
            var solicitud = await _context.Solicitudes
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
                return NotFound();

            return View(solicitud);
        }
    }
}