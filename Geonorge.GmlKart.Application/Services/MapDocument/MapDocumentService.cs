using Geonorge.GmlKart.Application.Exceptions;
using Geonorge.GmlKart.Application.HttpClients;
using Geonorge.GmlKart.Application.Models.Map;
using Geonorge.GmlKart.Application.Models.Validation;
using Microsoft.AspNetCore.Http;
using System.Xml.Linq;
using Wmhelp.XPath2;

namespace Geonorge.GmlKart.Application.Services
{
    public class MapDocumentService : IMapDocumentService
    {
        private readonly IValidationHttpClient _validationHttpClient;
        private readonly IGmlToGeoJsonService _gmlToGeoJsonService;

        public MapDocumentService(
            IValidationHttpClient validationHttpClient,
            IGmlToGeoJsonService gmlToGeoJsonService)
        {
            _validationHttpClient = validationHttpClient;
            _gmlToGeoJsonService = gmlToGeoJsonService;
        }

        public async Task<MapDocument> CreateMapDocumentAsync(IFormFile file, bool validate)
        {
            ValidationResult validationResult;
            
            if (validate)
            {
                validationResult = await _validationHttpClient.ValidateAsync(file);

                if (!validationResult.XsdValidated || !validationResult.EpsgValidated)
                {
                    return new MapDocument
                    {
                        FileName = file.FileName,
                        FileSize = file.Length,
                        ValidationResult = validationResult
                    };
                }
            }
            else
            {
                validationResult = new();
            }

            var document = await LoadXDocumentAsync(file);

            if (document == null)
                return null;

            return new MapDocument
            {
                FileName = file.FileName,
                FileSize = file.Length,
                Epsg = GetEpsg(document),
                GeoJson = _gmlToGeoJsonService.CreateGeoJsonDocument(document),
                ValidationResult = validationResult
            };
        }

        private static Epsg GetEpsg(XDocument document)
        {
            var srsName = document.XPath2SelectOne<XAttribute>("(//*[@srsName]/@srsName)[1]")?.Value;

            if (srsName == null)
                return null;

            return Epsg.FromSrsName(srsName);
        }

        private static async Task<XDocument> LoadXDocumentAsync(IFormFile file)
        {
            try
            {
                return await XDocument.LoadAsync(file.OpenReadStream(), LoadOptions.None, default);
            }
            catch (Exception exception)
            {
                throw new CouldNotLoadXDocumentException("Kunne ikke laste plankartet.", exception);
            }
        }
    }
}
