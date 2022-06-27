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

        public static readonly Dictionary<string, string> EpsgCodes = new()
        {
            { "25832", "UTM sone 32 (EUREF89)" },
            { "25833", "UTM sone 33 (EUREF89)" },
            { "25835", "UTM sone 35 (EUREF89)" },
            { "5972", "UTM sone 32 + NN2000 (EUREF89)" },
            { "5973", "UTM sone 33 + NN2000 (EUREF89)" },
            { "5975", "UTM sone 35 + NN2000 (EUREF89)" },
            { "5110", "NTM sone 10 (EUREF89)" },
            { "5111", "NTM sone 11 (EUREF89)" },
            { "5112", "NTM sone 12 (EUREF89)" },
            { "5113", "NTM sone 13 (EUREF89)" },
            { "5114", "NTM sone 14 (EUREF89)" },
            { "5115", "NTM sone 15 (EUREF89)" },
            { "5116", "NTM sone 16 (EUREF89)" },
            { "5117", "NTM sone 17 (EUREF89)" },
            { "5118", "NTM sone 18 (EUREF89)" },
            { "5119", "NTM sone 19 (EUREF89)" },
            { "5120", "NTM sone 20 (EUREF89)" },
            { "5121", "NTM sone 21 (EUREF89)" },
            { "5122", "NTM sone 22 (EUREF89)" },
            { "5123", "NTM sone 23 (EUREF89)" },
            { "5124", "NTM sone 24 (EUREF89)" },
            { "5125", "NTM sone 25 (EUREF89)" },
            { "5126", "NTM sone 26 (EUREF89)" },
            { "5127", "NTM sone 27 (EUREF89)" },
            { "5128", "NTM sone 28 (EUREF89)" },
            { "5129", "NTM sone 29 (EUREF89)" },
            { "5130", "NTM sone 30 (EUREF89)" },
            { "5950", "NTM sone 10 + NN2000 (EUREF89)" },
            { "5951", "NTM sone 11 + NN2000 (EUREF89)" },
            { "5952", "NTM sone 12 + NN2000 (EUREF89)" },
            { "5953", "NTM sone 13 + NN2000 (EUREF89)" },
            { "5954", "NTM sone 14 + NN2000 (EUREF89)" },
            { "5955", "NTM sone 15 + NN2000 (EUREF89)" },
            { "5956", "NTM sone 16 + NN2000 (EUREF89)" },
            { "5957", "NTM sone 17 + NN2000 (EUREF89)" },
            { "5958", "NTM sone 18 + NN2000 (EUREF89)" },
            { "5959", "NTM sone 19 + NN2000 (EUREF89)" },
            { "5960", "NTM sone 20 + NN2000 (EUREF89)" },
            { "5961", "NTM sone 21 + NN2000 (EUREF89)" },
            { "5962", "NTM sone 22 + NN2000 (EUREF89)" },
            { "5963", "NTM sone 23 + NN2000 (EUREF89)" },
            { "5964", "NTM sone 24 + NN2000 (EUREF89)" },
            { "5965", "NTM sone 25 + NN2000 (EUREF89)" },
            { "5966", "NTM sone 26 + NN2000 (EUREF89)" },
            { "5967", "NTM sone 27 + NN2000 (EUREF89)" },
            { "5968", "NTM sone 28 + NN2000 (EUREF89)" },
            { "5969", "NTM sone 29 + NN2000 (EUREF89)" },
            { "5970", "NTM sone 30 + NN2000 (EUREF89)" },
        };
    }
}
