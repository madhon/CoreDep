using Microsoft.AspNetCore.HttpOverrides;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o => o.AddServerHeader = false);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddControllersWithViews();


var app = builder.Build();


app.UseSecurityHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    //app.UsePathBase("/coredep");
    app.UseExceptionHandler("/Home/Error");
}

app.UseForwardedHeaders();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


public static partial class Program
{
    public static readonly ActivitySource ActivitySource = new ActivitySource("CoreDep");
}
