using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using StudentManagment;
using StudentManagment.Middleware;
using StudentManagment.Services;
using StudentManagment.Services.Interface;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StudentManagement_API.Services.CacheService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<CustomExceptionFilter>();
    options.Filters.Add(new UnauthorizedExceptionFilter());
    options.Filters.Add(new AccessViolationFilter());
}).AddNewtonsoftJson(option =>
{
    option.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    option.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
}).AddXmlDataContractSerializerFormatters();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ICacheServices,CacheServices>();
builder.Services.AddScoped<CustomExceptionFilter>();
builder.Services.AddSingleton<IHttpContextAccessor,HttpContextAccessor>();
builder.Services.AddSingleton<IExceptionFilter, CustomExceptionFilter>();
builder.Services.AddDataLayerServices();
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2); // Set session timeout
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // This will handle exceptions and redirect to the specified error page.
    app.UseExceptionHandler("/Home/Error");
}
app.UseMiddleware<CustomHeaderMiddleWare>();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
