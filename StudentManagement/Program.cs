
using Asp.Versioning;
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
var configuration = builder.Configuration;
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


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
            };
        });

builder.Services.AddHttpClient();
builder.Services.AddApiVersioning(setup =>
{
    setup.DefaultApiVersion = new ApiVersion(1, 0);
    setup.AssumeDefaultVersionWhenUnspecified = true;
    setup.ReportApiVersions = true;
    setup.ApiVersionReader = new HeaderApiVersionReader("Api-Version");
}).AddMvc();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CustomHeaderMiddleWare>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
