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
        [RequestFormLimits(MultipartBodyLengthLimit = 26_214_400)]
        [RequestSizeLimit(26_214_400)]
        public async Task<IActionResult> CreateMapDocument()
        {
            try
            {
                var formData = await _multipartRequestService.GetFileFromMultipart();

                if (formData == null)
                    return BadRequest();

                var document = await _mapDocumentService.CreateMapDocumentAsync(formData.File, formData.Validate);
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
