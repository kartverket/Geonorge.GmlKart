using Geonorge.GmlKart.Application.Models.Map;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OSGeo.OGR;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Wmhelp.XPath2;

namespace Geonorge.GmlKart.Application.Services
{
    public class GmlToGeoJsonService : IGmlToGeoJsonService
    {
        private static readonly XNamespace _gmlNs = "http://www.opengis.net/gml/3.2";
        private static readonly XmlNamespaceManager _namespaceManager = CreateNamespaceManager();
        
        public GeoJsonFeatureCollection CreateGeoJsonDocument(XDocument document, Dictionary<string, string> geoElementMappings = null)
        {
            if (document == null)
                return null;

            var featureMembers = GetFeatureMembers(document);
            var featureCollection = new GeoJsonFeatureCollection();

            foreach (var featureMember in featureMembers)
            {
                var geoElement = GetGeometryElement(featureMember, geoElementMappings);

                if (geoElement == null)
                    continue;

                using var geometry = GetGeometry(geoElement);

                if (geometry == null)
                    continue;

                var feature = new GeoJsonFeature { Geometry = GetGeometry(geometry) };

                geoElement.Remove();

                feature.Properties = CreateProperties(featureMember, featureMember.Name.LocalName, featureMember.Attribute(_gmlNs + "id").Value);

                featureCollection.Features.Add(feature);
            }

            return featureCollection;
        }

        private static XElement GetGeometryElement(XElement featureMember, Dictionary<string, string> geoElementMappings)
        {
            if (geoElementMappings != null && geoElementMappings.TryGetValue(featureMember.Name.LocalName, out var xPath))
                xPath = $"*:{xPath}/*";
            else
                xPath = "*/gml:*";

            return featureMember.XPath2SelectElement(xPath, _namespaceManager);
        }

        private static Geometry GetGeometry(XElement geoElement)
        {
            if (!TryCreateGeometry(geoElement, out var tempGeometry))
                return null;

            if (!geoElement.Descendants(_gmlNs + "Arc").Any())
                return tempGeometry;

            Geometry geometry = null;
            var geometryType = tempGeometry.GetGeometryType();

            switch (geometryType)
            {
                case wkbGeometryType.wkbCircularString:
                    geometry = Ogr.ForceToLineString(tempGeometry);
                    break;
                case wkbGeometryType.wkbSurface:
                    geometry = Ogr.ForceToPolygon(tempGeometry);
                    break;
                case wkbGeometryType.wkbMultiSurface:
                    geometry = Ogr.ForceToMultiPolygon(tempGeometry);
                    break;
                default:
                    break;
            }

            tempGeometry.Dispose();
            return geometry;
        }

        private static List<XElement> GetFeatureMembers(XDocument document)
        {
            var localName = document.Root.Elements()
                .Any(element => element.Name.LocalName == "featureMember") ? "featureMember" : "featureMembers";

            return document.Root.Elements()
                .Where(element => element.Name.LocalName == localName)
                .SelectMany(element => element.Elements())
                .ToList();
        }

        private static bool TryCreateGeometry(XElement geoElement, out Geometry geometry)
        {
            try
            {
                geometry = Geometry.CreateFromGML(geoElement.ToString());
                return true;
            }
            catch
            {
                geometry = null;
                return false;
            }
        }

        private static GeoJsonGeometry GetGeometry(Geometry geometry)
        {
            var json = geometry.ExportToJson(Array.Empty<string>());

            if (json == null)
                return null;

            var jObject = JObject.Parse(json);
            var type = jObject["type"].ToString();
            var coordinates = jObject["coordinates"].ToString();

            if (type == GeometryType.Point)
            {
                return new Point(type, JsonConvert.DeserializeObject<double[]>(coordinates));
            }

            if (type == GeometryType.MultiPoint)
            {
                return new MultiPoint(type, JsonConvert.DeserializeObject<double[][]>(coordinates));
            }

            if (type == GeometryType.LineString)
            {
                return new LineString(type, JsonConvert.DeserializeObject<double[][]>(coordinates));
            }

            if (type == GeometryType.MultiLineString)
            {
                return new MultiLineString(type, JsonConvert.DeserializeObject<double[][][]>(coordinates));
            }

            if (type == GeometryType.Polygon)
            {
                return new Polygon(type, JsonConvert.DeserializeObject<double[][][]>(coordinates));
            }

            if (type == GeometryType.MultiPolygon)
            {
                return new MultiPolygon(type, JsonConvert.DeserializeObject<double[][][][]>(coordinates));
            }

            return null;
        }

        private static JObject CreateProperties(XElement featureMember, string featureName, string gmlId)
        {
            featureMember.Name = "values";

            var builder = new StringBuilder();
            using var writer = new StringWriter(builder);

            var serializer = JsonSerializer.Create();
            serializer.Serialize(new GmlToJsonWriter(writer), featureMember);

            var jObject = JObject.Parse(builder.ToString());
            var values = jObject["values"] as JObject;

            values.Add(new JProperty("name", featureName));
            values.Add(new JProperty("label", $"{featureName} '{gmlId}'"));

            return jObject["values"] as JObject;
        }

        private static XmlNamespaceManager CreateNamespaceManager()
        {
            var namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("gml", _gmlNs.NamespaceName);

            return namespaceManager;
        }
    }
}
