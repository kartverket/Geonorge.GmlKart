using Geonorge.GmlKart.Application.Models;

namespace Geonorge.GmlKart.Application.Services
{
    public interface IMultipartRequestService
    {
        Task<FormData> GetFileFromMultipart();
    }
}
