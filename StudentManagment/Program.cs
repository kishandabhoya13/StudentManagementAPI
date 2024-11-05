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
app.UseWebSockets();

//app.Map("/", async httpContext =>
//{
//    if (httpContext.WebSockets.IsWebSocketRequest is false)
//        httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

//    else
//    {
//        using var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();

//        while (true)
//        {
//            var data = Encoding.ASCII.GetBytes($"Data {DateTime.Now}");
//            await webSocket.SendAsync(data, WebSocketMessageType.Text, false, CancellationToken.None);
//            await Task.Delay(5000);
//        }
//    }
//});
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
