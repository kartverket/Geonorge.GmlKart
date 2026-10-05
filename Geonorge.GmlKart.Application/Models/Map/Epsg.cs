using OSGeo.OSR;

namespace Geonorge.GmlKart.Application.Models.Map
{
    public class Epsg
    {
        // Nyere PROJ-database (GDAL 3.13+) definerer UTM + NN2000 med ETRS89-NOR (EPSG:1102x) som horisontal del.
        // Koordinatene er identiske med ETRS89 / UTM (EPSG:2583x), som er det frontend støtter.
        private static readonly Dictionary<string, string> _horizontalCodeAliases = new()
        {
            { "11022", "25832" },
            { "11023", "25833" },
            { "11025", "25835" }
        };

        public string Code { get; private set; }
        public string Code2d { get; private set; }
        public string Description { get; private set; }

        public static Epsg Create(string epsgString)
        {
            if (!int.TryParse(epsgString, out var epsgCode))
                return null;

            var epsg = new Epsg
            {
                Code = $"EPSG:{epsgString}",
                Code2d = $"EPSG:{epsgString}"
            };

            try
            {
                using var spatialReference = new SpatialReference(null);
                spatialReference.ImportFromEPSG(epsgCode);

                epsg.Description = spatialReference.GetName();

                if (spatialReference.IsCompound() != 1)
                    return epsg;

                var projCsString = spatialReference.GetAuthorityCode("projcs");

                if (projCsString != null)
                    epsg.Code2d = $"EPSG:{_horizontalCodeAliases.GetValueOrDefault(projCsString, projCsString)}";

                return epsg;
            }
            catch
            {
                return null;
            }
        }
    }
}
