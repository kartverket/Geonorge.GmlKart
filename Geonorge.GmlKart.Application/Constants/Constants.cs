using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Xml.Linq;

namespace Geonorge.GmlKart.Application.Constants
{
    public class Constants
    {
        public static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new()
        {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
        };

        public static readonly XNamespace GmlNs = "http://www.opengis.net/gml/3.2";

        public static readonly string[] GmlGeometryElementNames = new[]
        {
            "CompositeCurve",
            "CompositeSolid",
            "CompositeSurface",
            "Curve",
            "GeometricComplex",
            "Grid",
            "LineString",
            "MultiCurve",
            "MultiGeometry",
            "MultiPoint",
            "MultiSolid",
            "MultiSurface",
            "OrientableCurve",
            "OrientableSurface",
            "Point",
            "Polygon",
            "PolyhedralSurface",
            "RectifiedGrid",
            "Solid",
            "Surface",
            "Tin",
            "TriangulatedSurface"
        };
    }
}
