using FlickrNet;
using HertejDB.Server;
using HertejDB.Server.Crawling;
using HertejDB.Server.Data;
using HertejDB.Server.Jobs;
using HertejDB.Server.Options;
using HertejDB.Server.Services;
using HertejDB.Server.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var authConfig = builder.Configuration.GetSection("Authorization").Get<AuthOptions>()!;
var dbConnString = builder.Configuration.GetSection("Database").Get<string>()!;

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options => {
	var apiinfo = new OpenApiInfo();

	if (!string.IsNullOrWhiteSpace(authConfig.Authority)) {
		var securityScheme = new OpenApiSecurityScheme {
			//Name = "OpenID",
			Type = SecuritySchemeType.OpenIdConnect,
			OpenIdConnectUrl = new Uri(authConfig.GetDiscoveryDocument())
		};
		
		var securityRequirements = new OpenApiSecurityRequirement {
			{ securityScheme, new string[] { } }
		};

		options.AddSecurityDefinition("OpenID", securityScheme);
		// Make sure swagger UI requires a Bearer token to be specified
		options.AddSecurityRequirement(securityRequirements);
	}

	options.SwaggerDoc("v1", apiinfo);
	
	options.OperationFilter<MethodNeedsAuthorizationFilter>();
});

builder.Services.AddDbContext<HertejDbContext>(dbcob => dbcob.UseNpgsql(dbConnString));

builder.Services.Configure<RatingOptions>(builder.Configuration.GetRequiredSection("Rating"));
builder.Services.Configure<LocalFileStorage.Options>(builder.Configuration.GetRequiredSection("Storage"));
builder.Services.Configure<FlickrOptions>(builder.Configuration.GetRequiredSection("Flickr"));

builder.Services.AddSingleton<FileStorage, LocalFileStorage>();
builder.Services.AddSingleton<CompleteRatingsJob>();
builder.Services.AddSingleton<HttpClient>();

builder.Services.AddSingleton(isp => {
	FlickrOptions options = isp.GetRequiredService<IOptions<FlickrOptions>>().Value;
	return new Flickr(options.ApiKey, options.ApiSecret);
});
builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ImageAcquirer, FlickrImageAcquirer>());

builder.Services.AddScoped<CrawlService>();
builder.Services.AddScoped<ImageService>();

builder.Services.AddScheduler(ctx => {
	ctx.AddJob<CompleteRatingsJob>(configure: so => so.CronSchedule = "0 * * * *");
	ctx.AddJob<CrawlImagesJob>(configure: so => so.CronSchedule = "0 * * * *");
});

builder.Services
	.AddAuthentication("Bearer")
	.AddJwtBearer("Bearer", options => {
		options.Authority = authConfig.Authority;
		options.RequireHttpsMetadata = true;
		options.TokenValidationParameters = new TokenValidationParameters {
			ValidateAudience = true,
			ValidAudiences = authConfig.Audiences
		};
		options.MetadataAddress = authConfig.GetDiscoveryDocument();
	});

builder.Services.AddAuthorization(options => {
	options.AddPolicy("Upload", policy => {
		if (string.IsNullOrWhiteSpace(authConfig.Authority)) {
			policy.RequireAssertion(ahc => true);
		} else {
			policy.RequireAuthenticatedUser();
			policy.RequireRole(authConfig.UploadRole);
		}
	});

	options.AddPolicy("Rate", policy => {
		if (string.IsNullOrWhiteSpace(authConfig.Authority)) {
			policy.RequireAssertion(ahc => true);
		} else {
			policy.RequireAuthenticatedUser();
			policy.RequireRole(authConfig.RateRole);
		}
	});

	options.AddPolicy("Admin", policy => {
		if (string.IsNullOrWhiteSpace(authConfig.Authority)) {
			policy.RequireAssertion(ahc => true);
		} else {
			policy.RequireAuthenticatedUser();
			policy.RequireRole(authConfig.AdminRole);
		}
	});
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (IServiceScope scope = app.Services.CreateScope()) {
	scope.ServiceProvider.GetRequiredService<HertejDbContext>().Database.Migrate();
}

app.Run();
