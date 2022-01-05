using Geonorge.GmlKart.Application.Models.Map;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OSGeo.OGR;
using System.Text;
using System.Xml.Linq;
using static Geonorge.GmlKart.Application.Constants.Constants;

namespace Geonorge.GmlKart.Application.Services
{
    public class GmlToGeoJsonService : IGmlToGeoJsonService
    {
        public GeoJsonFeatureCollection CreateGeoJsonDocument(XDocument document, Dictionary<string, string> geoElementMappings = null)
        {
            if (document == null)
                return null;

            var featureMembers = GetFeatureMembers(document);
            var featureCollection = new GeoJsonFeatureCollection();

            foreach (var featureMember in featureMembers)
            {
                var geoElements = GetGeometryElements(featureMember);
                var primaryGeoElement = GetPrimaryGeometryElement(featureMember.Name.LocalName, geoElements, geoElementMappings);

                if (primaryGeoElement.Value == null)
                    continue;

                using var geometry = GetGeometry(primaryGeoElement.Value);

                if (geometry == null)
                    continue;

                geoElements.Remove(primaryGeoElement.Key);
                primaryGeoElement.Value.Parent.Remove();

                var feature = new GeoJsonFeature { Geometry = CreateGeoJson(geometry) };
                var otherGeoJsonGeometries = CreateOtherGeoJsonGeometries(geoElements);

                feature.Properties = CreateProperties(
                    featureMember, 
                    featureMember.Name.LocalName, 
                    featureMember.Attribute(GmlNs + "id").Value, 
                    otherGeoJsonGeometries
                );

                featureCollection.Features.Add(feature);
            }

            return featureCollection;
        }

        public static Dictionary<string, XElement> GetGeometryElements(XElement featureMember)
        {
            return featureMember.Descendants()
                .Where(element => GmlGeometryElementNames.Contains(element.Name.LocalName) &&
                    element.Parent.Name.Namespace != element.Parent.GetNamespaceOfPrefix("gml"))
                .Select(element => (element.Parent.Name.LocalName, element))
                .ToDictionary(tuple => tuple.LocalName, tuple => tuple.element);
        }

        private static KeyValuePair<string, XElement> GetPrimaryGeometryElement(
            string featureName, Dictionary<string, XElement> geoElements, Dictionary<string, string> geoElementMappings = null)
        {
            if (!geoElements.Any())
                return default;

            if (geoElements.Count == 1)
                return geoElements.First();

            if (geoElementMappings == null || !geoElementMappings.TryGetValue(featureName, out var elementName))
                return geoElements.First();

            return geoElements.SingleOrDefault(kvp => kvp.Key == elementName);
        }

        private static Dictionary<string, GeoJsonGeometry> CreateOtherGeoJsonGeometries(Dictionary<string, XElement> geoElements)
        {
            var otherGeometries = new Dictionary<string, GeoJsonGeometry>();

            if (!geoElements.Any())
                return otherGeometries;

            foreach (var (elementName, geoElement) in geoElements)
            {
                using var geometry = GetGeometry(geoElement);

                if (geometry == null)
                    continue;

                var geoJson = CreateGeoJson(geometry);
                otherGeometries.Add(elementName, geoJson);
                geoElement.Parent.Remove();
            }

            return otherGeometries;
        }

        private static Geometry GetGeometry(XElement geoElement)
        {
            if (!TryCreateGeometry(geoElement, out var tempGeometry))
                return null;

            if (!geoElement.Descendants(GmlNs + "Arc").Any())
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

        private static GeoJsonGeometry CreateGeoJson(Geometry geometry)
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

        private static JObject CreateProperties(
            XElement featureMember, string featureName, string gmlId, Dictionary<string, GeoJsonGeometry> otherGeometries)
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

            foreach (var (propName, geoJson) in otherGeometries)
            {
                var geoJsonJObject = JObject.Parse(JsonConvert.SerializeObject(geoJson, DefaultJsonSerializerSettings));
                values.Add(new JProperty(propName, geoJsonJObject));
            }

            return jObject["values"] as JObject;
        }
    }
}
