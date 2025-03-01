using Demo.App.Server;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Some API v1", Version = "v1" });
    options.AddSignalRSwaggerGen();
});

builder.Services.AddSignalR();
builder.Services.AddSingleton<WorkerService>();
builder.Services.AddSingleton<DataHub>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<WorkerService>());

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

app.MapHub<DataHub>("/dataHub");

app.MapFallbackToFile("/index.html");

await app.RunAsync();
