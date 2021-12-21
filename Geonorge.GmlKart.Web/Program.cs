using Geonorge.GmlKart.Application.HttpClients;
using Geonorge.GmlKart.Application.Services;
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
    //options.MimeTypes = new[] { "text/plain" };
    options.Providers.Add<GzipCompressionProvider>();
});

services.AddControllers();

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddTransient<IGmlToGeoJsonService, GmlToGeoJsonService>();
services.AddTransient<IMapDocumentService, MapDocumentService>();
services.AddHttpClient<IValidationHttpClient, ValidationHttpClient>();

services.Configure<ValidationSettings>(configuration.GetSection(ValidationSettings.SectionName));

var app = builder.Build();

Ogr.RegisterAll();
Ogr.UseExceptions();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(options => options
    .AllowAnyOrigin()
    .AllowAnyMethod()
);

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseResponseCompression();

app.MapControllers();

app.Run();
