using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Usuarios.DataAccess.Entities;

[Table("localizaciones")]
public class LocalizacionEntity
{
    [Key]
    public int LocalizacionId { get; set; }

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    [Column(TypeName = "geometry(Polygon)")]
    public NetTopologySuite.Geometries.Polygon? Poligono { get; set; }
}
