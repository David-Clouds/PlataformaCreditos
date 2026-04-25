using System.ComponentModel.DataAnnotations;

namespace PlataformaCreditos.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        public string UsuarioId { get; set; } = "";

        [Range(1, double.MaxValue)]
        public decimal IngresosMensuales { get; set; }

        public bool Activo { get; set; }
    }
}