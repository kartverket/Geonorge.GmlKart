using Geonorge.GmlKart.Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace Geonorge.GmlKart.Application.Services
{
    public class MultipartRequestService : IMultipartRequestService
    {
        private static readonly Regex _gml32Regex = new(@"^<\?xml.*?<gml:FeatureCollection.*?xmlns:gml=""http:\/\/www\.opengis\.net\/gml\/3\.2""", RegexOptions.Compiled | RegexOptions.Singleline);

        private readonly IHttpContextAccessor _httpContextAccessor;

        public MultipartRequestService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<FormData> GetFileFromMultipart()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var reader = new MultipartReader(request.GetMultipartBoundary(), request.Body);
            var formData = new FormData();
            MultipartSection section;

            try
            {
                while ((section = await reader.ReadNextSectionAsync()) != null)
                {
                    if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition))
                        continue;

                    var name = contentDisposition.Name.Value;

                    if (contentDisposition.IsFileDisposition() && name == "gmlFile" && await IsGml32File(section) && formData.File == null)
                    {
                        formData.File = await CreateFormFile(contentDisposition, section);
                    }
                    else if (contentDisposition.IsFormDisposition() && name == "validate")
                    {
                        var value = await GetFormValue(section, contentDisposition);

                        if (bool.TryParse(value, out var validate))
                            formData.Validate = false;
                    }
                }
            }
            catch (Exception ec)
            {
                return null;
            }

            return formData.File != null ? formData : null;
        }

        private static async Task<IFormFile> CreateFormFile(ContentDispositionHeaderValue contentDisposition, MultipartSection section)
        {
            var memoryStream = new MemoryStream();
            await section.Body.CopyToAsync(memoryStream);
            await section.Body.DisposeAsync();
            memoryStream.Position = 0;

            return new FormFile(memoryStream, 0, memoryStream.Length, contentDisposition.Name.ToString(), contentDisposition.FileName.ToString())
            {
                Headers = new HeaderDictionary(),
                ContentType = section.ContentType
            };
        }

        private static async Task<string> GetFormValue(MultipartSection section, ContentDispositionHeaderValue contentDisposition)
        {
            using var streamReader = new StreamReader(section.Body, GetEncoding(section), true, 1024, true);
            
            return await streamReader.ReadToEndAsync();
        }

        private static async Task<bool> IsGml32File(MultipartSection section)
        {
            var buffer = new byte[1000];
            await section.Body.ReadAsync(buffer.AsMemory(0, 1000));
            section.Body.Position = 0;

            using var memoryStream = new MemoryStream(buffer);
            using var streamReader = new StreamReader(memoryStream);
            var gmlString = streamReader.ReadToEnd();

            return _gml32Regex.IsMatch(gmlString);
        }

        private static Encoding GetEncoding(MultipartSection section)
        {
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);

            #pragma warning disable SYSLIB0001
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
            #pragma warning restore SYSLIB0001
            {
                return Encoding.UTF8;
            }

            return mediaType.Encoding;
        }
    }
}
