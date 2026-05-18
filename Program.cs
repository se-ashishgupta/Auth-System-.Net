using AuthSystem.Data;
using Microsoft.EntityFrameworkCore;
using AuthSystem.Services;
using AuthSystem.Middleware;
using AuthSystem.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))); // (Npgsql is the .NET driver for Postgres.)

// Services
builder.Services.AddApplicationServices();

// Authentication + Authorization
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Custom exception middleware - must be FIRST
app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();