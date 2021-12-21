using Geonorge.GmlKart.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Geonorge.GmlKart.Web.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        private readonly ILogger<ControllerBase> _logger;

        protected BaseController(
            ILogger<ControllerBase> logger)
        {
            _logger = logger;
        }

        protected IActionResult HandleException(Exception exception)
        {
            _logger.LogError(exception.ToString());

            return exception switch
            {
                CouldNotLoadXDocumentException or CouldNotValidateException => BadRequest(exception.Message),
                ArgumentException _ or InvalidDataException _ or FormatException _ => BadRequest(),
                Exception _ => StatusCode(StatusCodes.Status500InternalServerError),
                _ => null,
            };
        }
    }
}
