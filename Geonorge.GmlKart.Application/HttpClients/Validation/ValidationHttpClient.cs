using Geonorge.GmlKart.Application.Exceptions;
using Geonorge.GmlKart.Application.Models.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Geonorge.GmlKart.Application.HttpClients.Validation
{
    public class ValidationHttpClient : IValidationHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly ValidationSettings _settings;
        private readonly ILogger<ValidationHttpClient> _logger;

        public ValidationHttpClient(
            HttpClient httpClient,
            IOptions<ValidationSettings> options,
            ILogger<ValidationHttpClient> logger)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<ValidationResult> ValidateAsync(IFormFile file)
        {
            var report = await RunValidationAsync(file);
            var failedRules = report.Rules.Where(rule => rule.Status == "FAILED");
            var xsdRule = failedRules.SingleOrDefault(rule => rule.Id == _settings.XsdRuleId);
            var result = new ValidationResult { Id = report.CorrelationId };

            if (xsdRule != null)
            {
                result.XsdValidationMessages = xsdRule.Messages.Select(message => message.Message).ToList();
                return result;
            }

            var epsgRule = failedRules.SingleOrDefault(rule => rule.Id == _settings.EpsgRuleId);

            if (epsgRule != null)
            {
                result.EpsgValidationMessages = epsgRule.Messages.Select(message => message.Message).ToList();
                return result;
            }

            result.Rules = failedRules.ToList();

            return result;
        }

        private async Task<ValidationReport> RunValidationAsync(IFormFile file)
        {
            try
            {
                using var content = new MultipartFormDataContent { { new StreamContent(file.OpenReadStream()), "xmlFiles", file.FileName } };
                using var request = new HttpRequestMessage(HttpMethod.Post, _settings.ApiUrl) { Content = content };

                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<ValidationReport>(responseString);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Kunne ikke validere filen '{fileName}'!", file.FileName);
                throw new CouldNotValidateException($"Kunne ikke validere filen '{file.FileName}'!", exception);
            }
        }
    }
}
