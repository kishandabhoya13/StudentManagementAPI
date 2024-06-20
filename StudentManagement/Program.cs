
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using StudentManagement_API.Services;
using StudentManagment_API;
using StudentManagment_API.Middleware;
using StudentManagment_API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add<CustomExceptionFilter>();
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddMvc(options => {
    options.Filters.Add<LogActionFilter>();
    options.Filters.Add<CustomExceptionFilter>();
});

builder.Services.AddSwaggerGen();
builder.Services.AddDataLayerServices();
//builder.Services.AddScoped<IStudentServices, StudentServices>();
//builder.Services.AddScoped<IProfessorHodServices, ProfessorHodServices>();
//builder.Services.AddScoped<IJwtServices, JwtServices>();
builder.Services.AddScoped<CustomExceptionFilter>();
builder.Services.AddSingleton<IExceptionFilter, CustomExceptionFilter>();

var app = builder.Build();

app.UseMiddleware<CustomHeaderMiddleWare>();
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
