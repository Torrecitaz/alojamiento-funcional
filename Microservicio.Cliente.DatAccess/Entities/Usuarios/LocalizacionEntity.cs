using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microservicio.Cliente.DatAccess.Entities.Usuarios;

[Table("localizaciones")]
public class LocalizacionEntity
{
    [Key]
    public int LocalizacionId { get; set; }
    public string? Descripcion { get; set; }
}
