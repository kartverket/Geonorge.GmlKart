using Geonorge.GmlKart.Application.Helpers;
using Geonorge.GmlKart.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Geonorge.GmlKart.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MapDocumentController : BaseController
    {
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new()
        {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
        };

        private readonly IMapDocumentService _mapDocumentService;

        public MapDocumentController(
            IMapDocumentService mapDocumentService,
            ILogger<MapDocumentController> logger) : base(logger)
        {
            _mapDocumentService = mapDocumentService;
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 25_000_000)]
        [RequestSizeLimit(25_000_000)]
        public async Task<IActionResult> CreateMapDocument(IFormFile file)
        {
            try
            {
                if (file == null)
                    return BadRequest();

                if (!FileHelper.IsGmlFile(file))
                    return BadRequest($"Filen '{file.FileName}' er ikke en gyldig GML-fil.");

                var document = await _mapDocumentService.CreateMapDocumentAsync(file);
                var serialized = JsonConvert.SerializeObject(document, _jsonSerializerSettings);

                return Ok(serialized);
            }
            catch (Exception exception)
            {
                var result = HandleException(exception);

                if (result != null)
                    return result;

                throw;
            }
        }
    }
}
