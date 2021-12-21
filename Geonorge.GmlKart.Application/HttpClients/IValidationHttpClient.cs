using Geonorge.GmlKart.Application.Models.Validation;
using Microsoft.AspNetCore.Http;

namespace Geonorge.GmlKart.Application.HttpClients
{
    public interface IValidationHttpClient
    {
        Task<ValidationResult> ValidateAsync(IFormFile file);
    }
}
