
//using Serilog;
using MagicVilla_VillaAPI;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Repository;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//SQL dependency injection
builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection"));
    option.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});


//register repository in program.cs
builder.Services.AddScoped<IVillaRepository,VillaRepository>();
builder.Services.AddScoped<IVillaNumberRepository,VillaNumberRepository>();


builder.Services.AddScoped<IuserRepository, userRepository>();
//Add api versioning
builder.Services.AddApiVersioning(options =>
{
	options.AssumeDefaultVersionWhenUnspecified=true;
	options.DefaultApiVersion = new ApiVersion(1, 0);
	options.ReportApiVersions = true;
});
//to tell swaggerthat this version is provided n to use that we have to add AddVersionAPIExplorer
builder.Services.AddVersionedApiExplorer(options =>
{
	// this is string format use to format the api version to groupname
	options.GroupNameFormat = "'v'VVV";
	options.SubstituteApiVersionInUrl = true;
});

//Inject Automapper dependency
builder.Services.AddAutoMapper(typeof(MappingConfig));
//Configure caching
builder.Services.AddResponseCaching();
//Configuring security we have add identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

//Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().
//WriteTo.File("log/villaLogs.txt", rollingInterval: RollingInterval.Day).CreateLogger();

// builder.Host.UseSerilog();
//Toretrieve key from appsetting.json
var key = builder.Configuration.GetValue<string>("ApiSettings:Secret");


builder.Services.AddAuthentication(x =>
{
	x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(x =>
		{
			x.RequireHttpsMetadata = false;
			x.SaveToken = true;
			x.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
				ValidateIssuer = false,
				ValidateAudience = false
			};
		});


builder.Services.AddControllers(option =>
{
	option.CacheProfiles.Add("Default20",
		new CacheProfile()
		{
			Duration = 20
		});
    //option.ReturnHttpNotAcceptable=true;
}).AddNewtonsoftJson().AddXmlDataContractSerializerFormatters();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Description =
			"JWT Authorization header using the Bearer scheme. \r\n\r\n " +
			"Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
			"Example: \"Bearer 12345abcdef\"",
		Name = "Authorization",
		In = ParameterLocation.Header,
		Scheme = "Bearer"
	});
	options.AddSecurityRequirement(new OpenApiSecurityRequirement()
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = "Bearer"
							},
				Scheme = "oauth2",
				Name = "Bearer",
				In = ParameterLocation.Header
			},
			new List<string>()
		}
	});
	options.SwaggerDoc("v1", new OpenApiInfo
	{
		Version = "v1.0",
		Title = "Magic Villa v1",
		Description = "API TO MANAGE VILLA",
		TermsOfService = new Uri("https://abcd.com/terms"),
		Contact = new OpenApiContact
		{
			Name = "NetMastery",
			Url = new Uri("https://dotnetmastery.com")
		},
		License = new OpenApiLicense
		{
			Name = "license",
			Url = new Uri("https://abcd.com/terms")
		}

	});
	options.SwaggerDoc("v2", new OpenApiInfo
	{
		Version = "v2.0",
		Title = "Magic Villa v2",
		Description = "API TO MANAGE VILLA",
		TermsOfService = new Uri("https://abcd.com/terms"),
		Contact = new OpenApiContact
		{
			Name = "NetMastery",
			Url = new Uri("https://dotnetmastery.com")
		},
		License = new OpenApiLicense
		{
			Name = "license",
			Url = new Uri("https://abcd.com/terms")
		}

	});
});




var app = builder.Build();

	// Configure the HTTP request pipeline.
	if (app.Environment.IsDevelopment())
	{
		app.UseSwagger();
		app.UseSwaggerUI( options =>
		{
			options.SwaggerEndpoint("/swagger/v1/swagger.json", "Magic_villaV1");
			options.SwaggerEndpoint("/swagger/v2/swagger.json", "Magic_villaV2");
		});
	}

	app.UseHttpsRedirection();

	app.UseAuthentication();
	app.UseAuthorization();

	app.MapControllers();

	app.Run();


