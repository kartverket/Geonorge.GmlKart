﻿using Geonorge.GmlKart.Application.Models.Map;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OSGeo.OGR;
using OSGeo.OSR;
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
            var ct = GetCoordinateTransformation(6172, 23035);

            Parallel.ForEach(featureMembers, featureMember =>
            {
                var geoElements = GetGeometryElements(featureMember);
                var primaryGeoElement = GetPrimaryGeometryElement(featureMember.Name.LocalName, geoElements, geoElementMappings);

                if (primaryGeoElement.Value == null)
                    return;

                using var geometry = GetGeometry(primaryGeoElement.Value, ct);

                if (geometry == null)
                    return;

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
            });

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

        private static Dictionary<string, JObject> CreateOtherGeoJsonGeometries(Dictionary<string, XElement> geoElements)
        {
            var otherGeometries = new Dictionary<string, JObject>();

            if (!geoElements.Any())
                return otherGeometries;

            foreach (var (elementName, geoElement) in geoElements)
            {
                using var geometry = GetGeometry(geoElement, null);

                if (geometry == null)
                    continue;

                var geoJson = CreateGeoJson(geometry);
                otherGeometries.Add(elementName, geoJson);
                geoElement.Parent.Remove();
            }

            return otherGeometries;
        }

        public static CoordinateTransformation GetCoordinateTransformation(int srcEpsg, int destEpsg)
        {
            using var srcSr = new SpatialReference(null);
            srcSr.ImportFromEPSG(srcEpsg);

            using var destSr = new SpatialReference(null);
            destSr.ImportFromEPSG(destEpsg);

            return new CoordinateTransformation(srcSr, destSr);
        }

        private static Geometry GetGeometry(XElement geoElement, CoordinateTransformation ct)
        {
            if (!TryCreateGeometry(geoElement, out var geometry))
                return null;

            /*if (ct != null)
                geometry.Transform(ct);*/

            var linearGeometry = geometry.GetLinearGeometry(0, Array.Empty<string>());
            
            geometry.Dispose();

            return linearGeometry;
        }

        private static List<XElement> GetFeatureMembers(XDocument document)
        {
            var featureMemberName = GetFeatureMemberName(document);

            return document.Root.Elements()
                .Where(element => element.Name.LocalName == featureMemberName)
                .SelectMany(element => element.Elements())
                .ToList();
        }

        private static string GetFeatureMemberName(XDocument document)
        {
            if (document.Root.Elements().Any(element => element.Name.LocalName == "featureMember"))
                return "featureMember";

            if (document.Root.Elements().Any(element => element.Name.LocalName == "featureMembers"))
                return "featureMembers";

            if (document.Root.Elements().Any(element => element.Name.LocalName == "member"))
                return "member";

            return "featureMember";
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

        private static JObject CreateGeoJson(Geometry geometry)
        {
            var json = geometry.ExportToJson(Array.Empty<string>());

            return JObject.Parse(json);
        }

        private static JObject CreateProperties(
            XElement featureMember, string featureName, string gmlId, Dictionary<string, JObject> otherGeometries)
        {
            featureMember.Name = "values";

            var builder = new StringBuilder();
            using var writer = new StringWriter(builder);

            var serializer = JsonSerializer.Create();
            serializer.Serialize(new GmlToJsonWriter(writer), featureMember);

            var jObject = JObject.Parse(builder.ToString());
            var values = jObject["values"] as JObject;

            values.Add(new JProperty("_name", featureName));
            values.Add(new JProperty("_label", $"{featureName} '{gmlId}'"));

            foreach (var (propName, geoJson) in otherGeometries)
                values.Add(new JProperty(propName, geoJson));

            return jObject["values"] as JObject;
        }
    }
}
