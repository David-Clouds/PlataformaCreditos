using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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


        public async Task<IActionResult> Index(string estado, decimal? minMonto, decimal? maxMonto, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var userId = _userManager.GetUserId(User);

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.UsuarioId == userId);

            if (cliente == null)
                return Content("No tienes cliente asociado");

            var query = _context.Solicitudes
                .Where(s => s.ClienteId == cliente.Id)
                .AsQueryable();


            if (!string.IsNullOrEmpty(estado))
                query = query.Where(s => s.Estado == estado);

            if (minMonto.HasValue)
                query = query.Where(s => s.MontoSolicitado >= minMonto);

            if (maxMonto.HasValue)
                query = query.Where(s => s.MontoSolicitado <= maxMonto);

            if (fechaInicio.HasValue)
                query = query.Where(s => s.FechaSolicitud >= fechaInicio);

            if (fechaFin.HasValue)
                query = query.Where(s => s.FechaSolicitud <= fechaFin);

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var solicitud = await _context.Solicitudes
                .Include(s => s.Cliente)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
                return NotFound();

            return View(solicitud);
        }
    }
}