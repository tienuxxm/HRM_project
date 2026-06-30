using System.Net;
using System.Reflection;
using Application;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssemblies(typeof(Program).GetTypeInfo().Assembly));
builder.Services.AddHealthChecks();
builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    options.LoginPath = "/Auth/Login-Screen";
    options.SlidingExpiration = true;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    context.Request.HttpContext.Request.Cookies.TryGetValue("X-Access-Token", out var token);
    if (!string.IsNullOrEmpty(token))
    {
        context.Request.Headers.Add("Authorization", "Bearer " + token);
    }

    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;

    if (response.StatusCode == (int)HttpStatusCode.Unauthorized ||
        response.StatusCode == (int)HttpStatusCode.Forbidden)
        response.Redirect("/auth/login-screen");
    await Task.CompletedTask;
});
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    /*endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=User}/{action=User}/{id?}");*/

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Dashboard}/{action=Index}/{id?}");

    endpoints.MapControllerRoute(
        name: "productCreate",
        pattern: "products/create",
        defaults: new { controller = "Products", action = "CreateProductView" });

    endpoints.MapControllerRoute(
        name: "productDetail",
        pattern: "products/{id}",
        defaults: new { controller = "Products", action = "Detail", id = "" });

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Categories}/{action=Categories}/{id?}");

    endpoints.MapFallback(context =>
    {
        context.Response.Redirect("/NotFound");
        return Task.CompletedTask;
    });
});


app.MapHealthChecks("/health");
System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

app.Run();