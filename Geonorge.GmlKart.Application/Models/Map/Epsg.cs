namespace Geonorge.GmlKart.Application.Models.Map
{
    public class Epsg
    {
        private static readonly Dictionary<string, string> _descriptions = new()
        {
            { "25832", "UTM sone 32 (EUREF89)" },
            { "25833", "UTM sone 33 (EUREF89)" },
            { "25835", "UTM sone 35 (EUREF89)" }
        };

        public string Code { get; set; }
        public string Description { get; set; }

        public Epsg()
        {
        }

        public Epsg(string code)
        {
            Code = $"EPSG:{code}";
            Description = _descriptions.ContainsKey(code) ? _descriptions[code] : null;
        }
    }
}
