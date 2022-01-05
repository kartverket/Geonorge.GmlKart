using Geonorge.GmlKart.Application.HttpClients;
using Geonorge.GmlKart.Application.Services;
using Geonorge.Validator.Web;
using Microsoft.AspNetCore.ResponseCompression;
using OSGeo.OGR;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});

services.AddControllers();

services.AddEndpointsApiExplorer();

services.AddSwaggerGen(options => options.OperationFilter<MultipartOperationFilter>());

services.AddHttpContextAccessor();

services.AddTransient<IGmlToGeoJsonService, GmlToGeoJsonService>();
services.AddTransient<IMapDocumentService, MapDocumentService>();
services.AddTransient<IMultipartRequestService, MultipartRequestService>();
services.AddHttpClient<IValidationHttpClient, ValidationHttpClient>();

services.Configure<ValidationSettings>(configuration.GetSection(ValidationSettings.SectionName));

var app = builder.Build();

Ogr.RegisterAll();
Ogr.UseExceptions();

app.UseSwagger();

app.UseSwaggerUI();

app.UseCors(options => options
    .AllowAnyOrigin()
    .AllowAnyMethod()
);

app.Use(async (context, next) => {
    context.Request.EnableBuffering();
    await next();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseResponseCompression();

app.MapControllers();

app.Run();
