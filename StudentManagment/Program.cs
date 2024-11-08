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
using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.AspNetCore.Builder;
using System.Net.WebSockets;
using System.Net;
using StudentManagment.CallHubs;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddLocalization(option => { option.ResourcesPath = "Resources"; });
builder.Services.AddMvc().
    AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix).AddDataAnnotationsLocalization();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCulture = new List<CultureInfo>
    {
        new CultureInfo("en"),
        new CultureInfo("es"),
        new CultureInfo("fr"),

    };
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCulture;
    options.SupportedUICultures = supportedCulture;
});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<CustomExceptionFilter>();
    options.Filters.Add(new UnauthorizedExceptionFilter());
    options.Filters.Add(new AccessViolationFilter());
}).AddNewtonsoftJson(option =>
{
    option.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    option.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
}).AddXmlDataContractSerializerFormatters().AddDataAnnotationsLocalization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ICacheServices, CacheServices>();
builder.Services.AddScoped<CustomExceptionFilter>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IExceptionFilter, CustomExceptionFilter>();
builder.Services.AddDataLayerServices();
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
builder.Services.AddSignalR().AddHubOptions<CallHub>(options =>
{
    options.EnableDetailedErrors = true;
}); ;
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2); // Set session timeout
});
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
else
{
    app.UseDeveloperExceptionPage();
    // This will handle exceptions and redirect to the specified error page.
}

//var supportedCultures = new[] { "en", "fr", "es" };
//var localizationOptions = new RequestLocalizationOptions().
//    SetDefaultCulture(supportedCultures[0]).
//    AddSupportedCultures(supportedCultures).
//    AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);    

app.UseWebSockets();

app.UseMiddleware<WebSocketMiddleware>();
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

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<CallHub>("/callHub");
});
app.Run();
