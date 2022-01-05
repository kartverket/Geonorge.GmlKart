using Geonorge.GmlKart.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static Geonorge.GmlKart.Application.Constants.Constants;

namespace Geonorge.GmlKart.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MapDocumentController : BaseController
    {
        private readonly IMapDocumentService _mapDocumentService;
        private readonly IMultipartRequestService _multipartRequestService;

        public MapDocumentController(
            IMapDocumentService mapDocumentService,
            IMultipartRequestService multipartRequestService,
            ILogger<MapDocumentController> logger) : base(logger)
        {
            _mapDocumentService = mapDocumentService;
            _multipartRequestService = multipartRequestService;
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 25_000_000)]
        [RequestSizeLimit(25_000_000)]
        public async Task<IActionResult> CreateMapDocument()
        {
            try
            {
                var file = await _multipartRequestService.GetFileFromMultipart();

                if (file == null)
                    return BadRequest();

                var document = await _mapDocumentService.CreateMapDocumentAsync(file);
                var serialized = JsonConvert.SerializeObject(document, DefaultJsonSerializerSettings);

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
