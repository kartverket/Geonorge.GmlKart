using Geonorge.GmlKart.Application.Exceptions;
using Geonorge.GmlKart.Application.HttpClients;
using Geonorge.GmlKart.Application.Models.Map;
using Geonorge.GmlKart.Application.Models.Validation;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Geonorge.GmlKart.Application.Services
{
    public class MapDocumentService : IMapDocumentService
    {
        private static readonly Regex _srsNameRegex = new(@"srsName=""(http:\/\/www\.opengis\.net\/def\/crs\/EPSG\/0\/|urn:ogc:def:crs:EPSG::)(?<epsg>\d+)""");
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
                Epsg = await GetEpsgAsync(file),
                GeoJson = _gmlToGeoJsonService.CreateGeoJsonDocument(document),
                ValidationResult = validationResult
            };
        }

        private static async Task<Epsg> GetEpsgAsync(IFormFile file)
        {
            var buffer = new byte[5000];
            await file.OpenReadStream().ReadAsync(buffer.AsMemory(0, 5000));
            
            using var memoryStream = new MemoryStream(buffer);
            using var streamReader = new StreamReader(memoryStream);
            var fileString = streamReader.ReadToEnd();
            var match = _srsNameRegex.Match(fileString);

            if (match == null)
                return null;

            return new Epsg(match.Groups["epsg"].Value);
        }

        private static async Task<XDocument> LoadXDocumentAsync(IFormFile file)
        {
            try
            {
                return await XDocument.LoadAsync(file.OpenReadStream(), LoadOptions.None, default);
            }
            catch (Exception exception)
            {
                throw new CouldNotLoadXDocumentException("Kunne ikke laste GML-filen.", exception);
            }
        }
    }
}
