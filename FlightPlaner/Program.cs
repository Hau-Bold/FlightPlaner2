using FlightPlaner.Data;
using FlightPlaner.Services.Contract;
using FlightPlaner.Services.Impl;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<IOpenStreetMapService,OpenStreetMapService>();
builder.Services.AddTransient<IOptimizationService, OptimizationService>();
builder.Services.AddTransient<IRandomProvider, RandomProvider>();

//use in memory database
builder.Services.AddDbContext<GPSDbContext>(options
    => options.UseInMemoryDatabase("GPS.Db"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//avoid cross origin blocked by browser!
app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.UseAuthorization();

app.MapControllers();

app.Run();
