using Geonorge.GmlKart.Application.Models.Map;
using System.Xml.Linq;

namespace Geonorge.GmlKart.Application.Services
{
    public interface IGmlToGeoJsonService
    {
        GeoJsonFeatureCollection CreateGeoJsonDocument(XDocument document, Dictionary<string, string> geoElementMappings = null);
    }
}
