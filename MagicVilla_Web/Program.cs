using MagicVilla_Web;
using MagicVilla_Web.Services;
using MagicVilla_Web.Services.IServices;

var builder = WebApplication.CreateBuilder(args);

//Inject Automapper dependency
builder.Services.AddAutoMapper(typeof(MappingConfig));

//Register HttpClient
builder.Services.AddHttpClient<IVillaService, VillaService>();
//Register VillaService to depencdency injection
builder.Services.AddScoped<IVillaService,VillaService>();

//Register HttpClient
builder.Services.AddHttpClient<IVillaNumberService, VillaNumberService>();
//Register VillaNumberService to depencdency injection
builder.Services.AddScoped<IVillaNumberService, VillaNumberService>();
// Add services to the container.
builder.Services.AddControllersWithViews();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
