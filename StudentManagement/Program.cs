
using StudentManagement_API.Services;
using StudentManagement_API.Services.Interface;
using StudentManagment_API;
using StudentManagment_API.Middleware;
using StudentManagment_API.Services;
using StudentManagment_API.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(typeof(MappingConfig));

builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IStudentServices, StudentServices>();
builder.Services.AddScoped<IProfessorHodServices, ProfessorHodServices>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<CustomExceptionFilter>();
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
