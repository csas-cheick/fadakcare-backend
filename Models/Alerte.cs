using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Alerte
    {
        [Key]
        public int Id { get; set; }

        public string Message { get; set; } = string.Empty;

        public DateTime DateEnvoi { get; set; } = DateTime.Now;

        public int ExpediteurId { get; set; }

        public int DestinataireId { get; set; }

        public string ExpediteurRole { get; set; } = string.Empty;

        public string DestinataireRole { get; set; } = string.Empty;
    }
}
