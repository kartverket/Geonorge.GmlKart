using static Geonorge.GmlKart.Application.Constants.Constants;

namespace Geonorge.GmlKart.Application.Models.Map
{
    public class Epsg
    {
        public string Code { get; set; }
        public string Description { get; set; }

        public Epsg()
        {
        }

        public Epsg(string code)
        {
            Code = $"EPSG:{code}";
            Description = EpsgCodes.ContainsKey(code) ? EpsgCodes[code] : null;
        }
    }
}
