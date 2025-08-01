using Microsoft.EntityFrameworkCore;
using IpManager.Data.Repository;
using IpManager.Domain.Models.Data;
using IpManager.Domain.Repository;
using IpManager.Domain.Service;
using IpManager.Service;
using IpManagerJob;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IIpService, IpService>();
builder.Services.AddScoped<IIpRepository, IpRepository>();
builder.Services.AddMemoryCache();

builder.Services.AddHostedService<BackgroundJob>();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration["ConnectionStrings:Conn"]));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
