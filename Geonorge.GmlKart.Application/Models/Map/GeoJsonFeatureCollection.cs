using System.Collections.Generic;

namespace Geonorge.GmlKart.Application.Models.Map
{
    public class GeoJsonFeatureCollection
    {
        public string Type { get; } = "FeatureCollection";
        public List<GeoJsonFeature> Features { get; set; } = new();
    }
}
