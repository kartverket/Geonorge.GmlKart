using OSGeo.OSR;

namespace Geonorge.GmlKart.Application.Models.Map
{
    public class Epsg
    {
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
                    epsg.Code2d = $"EPSG:{projCsString}";

                return epsg;
            }
            catch
            {
                return null;
            }
        }
    }
}
