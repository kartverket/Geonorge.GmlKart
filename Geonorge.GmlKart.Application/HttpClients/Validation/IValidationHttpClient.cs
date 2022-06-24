using Geonorge.GmlKart.Application.Models.Validation;
using Microsoft.AspNetCore.Http;

namespace Geonorge.GmlKart.Application.HttpClients.Validation
{
    public interface IValidationHttpClient
    {
        Task<ValidationResult> ValidateAsync(IFormFile file);
    }
}
