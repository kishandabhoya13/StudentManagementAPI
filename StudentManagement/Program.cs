
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StudentManagement_API.Services;
using StudentManagment_API;
using StudentManagment_API.Middleware;
using StudentManagment_API.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add<CustomExceptionFilter>();
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddMvc(options =>
{
    options.Filters.Add<LogActionFilter>();
    options.Filters.Add<CustomExceptionFilter>();
});

builder.Services.AddMemoryCache();
builder.Services.AddSwaggerGen();
builder.Services.AddDataLayerServices();

builder.Services.AddScoped<CustomExceptionFilter>();
builder.Services.AddSingleton<IExceptionFilter, CustomExceptionFilter>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseMiddleware<CustomHeaderMiddleWare>();
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
