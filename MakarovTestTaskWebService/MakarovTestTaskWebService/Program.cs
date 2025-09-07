using MakarovTestTaskWebService.Core.Models.DataBase;
using MakarovTestTaskWebService.Core.Services;
using MakarovTestTaskWebService.Models;
using Microsoft.AspNetCore.Identity;
using Npgsql;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IEmployeeManager, EmployeeService>();
builder.Services.AddScoped<IDepartmentManager, DepartmentService>();
builder.Services.AddScoped<ICustomEmployeeValidator<Employee>, CustomEmployeeValidator>();
// отключил валидацию для упрощения проверки

var connectionString = builder.Configuration.GetConnectionString("PostgreSqlConnection");
builder.Services.AddScoped<IDbConnection>(sp => new NpgsqlConnection(connectionString));
var databaseInitializer = new DatabaseInitializer(
    connectionString, 
    builder.Services.BuildServiceProvider().GetRequiredService<ILogger<DatabaseInitializer>>()
    );
await databaseInitializer.InitializeAsync();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
