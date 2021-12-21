namespace Geonorge.GmlKart.Application.Models.Validation
{
    public class ValidationRuleMessage
    {
        public string Message { get; set; }
        public string FileName { get; set; }
        public List<string> XPaths { get; set; }
        public List<string> GmlIds { get; set; }
        public string ZoomTo { get; set; }

        public ValidationRuleMessage(string message, string fileName, List<string> xPaths, List<string> gmlIds, string zoomTo)
        {
            Message = message;
            FileName = fileName;
            XPaths = xPaths;
            GmlIds = gmlIds;
            ZoomTo = zoomTo;
        }
    }
}
