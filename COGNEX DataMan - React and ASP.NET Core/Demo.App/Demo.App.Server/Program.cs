using Demo.App.Server;
using Demo.App.Server.Endpoints;
using Demo.App.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.Configure();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapHub<LoggingHub>("/logging");
app.MapHub<ImageHub>("/image");
app.MapHub<ScannerHub>("/scanner");

app.AddScannerEndpoints();
app.AddScannersEndpoints();

app.MapFallbackToFile("/index.html");

await app.RunAsync();
