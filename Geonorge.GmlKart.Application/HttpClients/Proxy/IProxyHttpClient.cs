using Microsoft.AspNetCore.Mvc;

namespace Geonorge.GmlKart.Application.HttpClients.Proxy
{
    public interface IProxyHttpClient
    {
        Task<FileContentResult> GetAsync(string url);
    }
}
