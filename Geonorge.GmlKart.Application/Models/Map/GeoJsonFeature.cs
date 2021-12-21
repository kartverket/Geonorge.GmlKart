using Newtonsoft.Json.Linq;

namespace Geonorge.GmlKart.Application.Models.Map
{
    public class GeoJsonFeature
    {
        public string Type { get; } = "Feature";
        public GeoJsonGeometry Geometry { get; set; }
        public JObject Properties { get; set; }
    }
}
