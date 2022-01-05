using Microsoft.AspNetCore.Http;

namespace Geonorge.GmlKart.Application.Services
{
    public interface IMultipartRequestService
    {
        Task<IFormFile> GetFileFromMultipart();
    }
}
