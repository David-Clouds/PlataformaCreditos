using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using PlataformaCreditos.Data;
using PlataformaCreditos.Models;

namespace PlataformaCreditos.Controllers
{
    [Authorize(Roles = "Analista")]
    public class AnalistaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnalistaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🔥 LISTA SOLO PENDIENTES
        public async Task<IActionResult> Index()
        {
            var solicitudes = await _context.Solicitudes
                .Include(s => s.Cliente)
                .Where(s => s.Estado == "Pendiente")
                .ToListAsync();

            return View(solicitudes);
        }

        // 🔥 APROBAR
        public async Task<IActionResult> Aprobar(int id)
        {
            var solicitud = await _context.Solicitudes
                .Include(s => s.Cliente)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
                return NotFound();

            // ❌ No procesar si ya no está pendiente
            if (solicitud.Estado != "Pendiente")
            {
                TempData["Error"] = "Solicitud ya procesada";
                return RedirectToAction("Index");
            }

            // ❌ Regla importante
            if (solicitud.MontoSolicitado > solicitud.Cliente.IngresosMensuales * 5)
            {
                TempData["Error"] = "No se puede aprobar: excede 5x ingresos";
                return RedirectToAction("Index");
            }

            solicitud.Estado = "Aprobado";

            await _context.SaveChangesAsync();

            TempData["Ok"] = "Solicitud aprobada";

            return RedirectToAction("Index");
        }

        // 🔥 RECHAZAR (GET)
        public async Task<IActionResult> Rechazar(int id)
        {
            var solicitud = await _context.Solicitudes.FindAsync(id);

            if (solicitud == null)
                return NotFound();

            return View(solicitud);
        }

        // 🔥 RECHAZAR (POST)
        [HttpPost]
        public async Task<IActionResult> Rechazar(int id, string motivo)
        {
            var solicitud = await _context.Solicitudes.FindAsync(id);

            if (solicitud == null)
                return NotFound();

            if (solicitud.Estado != "Pendiente")
            {
                TempData["Error"] = "Solicitud ya procesada";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(motivo))
            {
                TempData["Error"] = "Debe ingresar un motivo";
                return View(solicitud);
            }

            solicitud.Estado = "Rechazado";
            solicitud.MotivoRechazo = motivo;

            await _context.SaveChangesAsync();

            TempData["Ok"] = "Solicitud rechazada";

            return RedirectToAction("Index");
        }
    }
}