using Geonorge.GmlKart.Application.Models.Map;
using Microsoft.AspNetCore.Http;

namespace Geonorge.GmlKart.Application.Services
{
    public interface IMapDocumentService
    {
        Task<MapDocument> CreateMapDocumentAsync(IFormFile file, bool validate);
    }
}
