﻿using Geonorge.GmlKart.Application.Exceptions;
using Geonorge.GmlKart.Application.HttpClients.Validation;
using Geonorge.GmlKart.Application.Models.Config.Styling;
using Geonorge.GmlKart.Application.Models.Map;
using Geonorge.GmlKart.Application.Models.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OSGeo.OSR;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static Geonorge.GmlKart.Application.Helpers.Helpers;

namespace Geonorge.GmlKart.Application.Services
{
    public class MapDocumentService : IMapDocumentService
    {
        private static readonly Regex _srsNameRegex = new(@"srsName=""(http:\/\/www\.opengis\.net\/def\/crs\/EPSG\/0\/|urn:ogc:def:crs:EPSG::|EPSG:)(?<epsg>\d+)""");

        private readonly IValidationHttpClient _validationHttpClient;
        private readonly IGmlToGeoJsonService _gmlToGeoJsonService;
        private readonly StylingSettings _stylingSettings;

        public MapDocumentService(
            IValidationHttpClient validationHttpClient,
            IGmlToGeoJsonService gmlToGeoJsonService,
            IOptions<StylingSettings> options)
        {
            _validationHttpClient = validationHttpClient;
            _gmlToGeoJsonService = gmlToGeoJsonService;
            _stylingSettings = options.Value;
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
                        ValidationResult = validationResult,
                        Styling = GetMapStyling(file)
                    };
                }
            }
            else
            {
                validationResult = new();
            }
                        
            var document = await LoadXDocumentAsync(file);

            var mapDocument = new MapDocument
            {
                FileName = file.FileName,
                FileSize = file.Length,
                Epsg = await GetEpsgAsync(file),
                GeoJson = _gmlToGeoJsonService.CreateGeoJsonDocument(document),
                ValidationResult = validationResult,
                Styling = GetMapStyling(file)
            };

            if (mapDocument.Epsg == null)
                throw new MapDocumentException("GML-filen har ingen gyldig EPSG-kode.");

            if (!mapDocument.GeoJson.Features.Any())
                throw new MapDocumentException("GML-filen inneholder ingen gyldige features.");

            return mapDocument;
        }

        private MapStyling GetMapStyling(IFormFile file)
        {
            var @namespace = GetDefaultNamespace(file);
            var mapStyling = _stylingSettings.Specifications.SingleOrDefault(specification => specification.Value.Namespace == @namespace);

            if (!mapStyling.Equals(default(KeyValuePair<string, MapStyling>)))
                return mapStyling.Value;

            return null;
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

            return Epsg.Create(match.Groups["epsg"].Value);
        }



        private static async Task<XDocument> LoadXDocumentAsync(IFormFile file)
        {
            try
            {
                return await XDocument.LoadAsync(file.OpenReadStream(), LoadOptions.None, default);
            }
            catch (Exception exception)
            {
                throw new MapDocumentException("Kunne ikke lese GML-filen.", exception);
            }
        }
    }
}
