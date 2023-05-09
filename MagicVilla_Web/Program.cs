using MagicVilla_Web;
using MagicVilla_Web.Services;
using MagicVilla_Web.Services.IServices;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

//Inject Automapper dependency
builder.Services.AddAutoMapper(typeof(MappingConfig));

//Register HttpClient
builder.Services.AddHttpClient<IVillaService, VillaService>();
//Register VillaService to depencdency injection
builder.Services.AddScoped<IVillaService,VillaService>();

//Register AuthService to depencdency injection
builder.Services.AddHttpClient<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthService, AuthService>();
//Register HttpClient
builder.Services.AddHttpClient<IVillaNumberService, VillaNumberService>();
//Register VillaNumberService to depencdency injection
builder.Services.AddScoped<IVillaNumberService, VillaNumberService>();
 //Register IHttpContextAccessor used in layout.cshtml
 builder.Services.AddSingleton<IHttpContextAccessor,HttpContextAccessor>();
// Add services to the container.
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).
    AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.LoginPath = "/Auth/Login";
        options.LoginPath = "/Auth/AccessDenied";
        options.SlidingExpiration = true;
    });
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
