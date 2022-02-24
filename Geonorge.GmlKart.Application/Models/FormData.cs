using Microsoft.AspNetCore.Http;

namespace Geonorge.GmlKart.Application.Models
{
    public class FormData
    {
        public IFormFile File { get; set; }
        public bool Validate { get; set; } = true;
    }
}
