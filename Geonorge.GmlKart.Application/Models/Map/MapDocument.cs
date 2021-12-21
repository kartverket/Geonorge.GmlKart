using Geonorge.GmlKart.Application.Models.Validation;

namespace Geonorge.GmlKart.Application.Models.Map
{
    public class MapDocument
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public Epsg Epsg { get; set; }
        public GeoJsonFeatureCollection GeoJson { get; set; } = new();
        public ValidationResult ValidationResult { get; set; }
    }
}
