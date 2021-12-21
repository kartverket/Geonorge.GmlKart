namespace Geonorge.GmlKart.Application.HttpClients
{
    public class ValidationSettings
    {
        public static readonly string SectionName = "Validation";
        public string ApiUrl { get; set; }
        public string XsdRuleId { get; set; }
        public string EpsgRuleId { get; set; }
    }
}
