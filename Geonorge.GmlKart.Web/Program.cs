using Geonorge.GmlKart.Application.HttpClients.Proxy;
using Geonorge.GmlKart.Application.HttpClients.Validation;
using Geonorge.GmlKart.Application.Models.Config.Styling;
using Geonorge.GmlKart.Application.Services;
using Geonorge.Validator.Web;
using Microsoft.AspNetCore.ResponseCompression;
using OSGeo.OGR;
using Serilog;
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

services.AddResponseCaching();

services.AddControllers();

services.AddEndpointsApiExplorer();

services.AddSwaggerGen(options => options.OperationFilter<MultipartOperationFilter>());

services.AddHttpContextAccessor();

services.AddTransient<IGmlToGeoJsonService, GmlToGeoJsonService>();
services.AddTransient<IMapDocumentService, MapDocumentService>();
services.AddTransient<IMultipartRequestService, MultipartRequestService>();
services.AddHttpClient<IValidationHttpClient, ValidationHttpClient>();
services.AddHttpClient<IProxyHttpClient, ProxyHttpClient>();

services.Configure<ValidationSettings>(configuration.GetSection(ValidationSettings.SectionName));
services.Configure<StylingSettings>(configuration.GetSection("Styling"));

Ogr.RegisterAll();
Ogr.UseExceptions();

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

builder.Logging.AddSerilog(Log.Logger, true);

var app = builder.Build();

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

app.UseResponseCaching();

app.UseAuthorization();

app.UseResponseCompression();

app.MapControllers();

app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

app.Run();
