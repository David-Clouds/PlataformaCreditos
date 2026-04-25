using System.ComponentModel.DataAnnotations;

namespace PlataformaCreditos.Models
{
    public class SolicitudCredito
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        [Range(1, double.MaxValue)]
        public decimal MontoSolicitado { get; set; }

        public DateTime FechaSolicitud { get; set; }

        public string Estado { get; set; } = "";

        public string? MotivoRechazo { get; set; }
    }
}