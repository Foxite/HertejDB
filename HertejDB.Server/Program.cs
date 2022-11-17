using HertejDB.Server;
using HertejDB.Server.Data;
using HertejDB.Server.Jobs;
using HertejDB.Server.Options;
using HertejDB.Server.Storage;
using Microsoft.EntityFrameworkCore;
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
	var apiinfo = new OpenApiInfo {
		Title = "theta-CandidateAPI",
		Version = "v1",
		Description = "Candidate API for thetalentbot",
		Contact = new OpenApiContact {
			Name = "thetalentbot",
			Url = new Uri("https://thetalentbot.com/developers/contact")
		},
		License = new OpenApiLicense() {
			Name = "Commercial",
			Url = new Uri("https://thetalentbot.com/developers/license")
		}
	};

	var securityScheme = new OpenApiSecurityScheme {
		//Name = "OpenID",
		Type = SecuritySchemeType.OpenIdConnect,
		OpenIdConnectUrl = new Uri(authConfig.GetDiscoveryDocument())
	};

	var securityRequirements = new OpenApiSecurityRequirement() {
		{securityScheme, new string[] {  }},
	};

	options.SwaggerDoc("v1", apiinfo);
	options.AddSecurityDefinition("OpenID", securityScheme);
	// Make sure swagger UI requires a Bearer token to be specified
	options.AddSecurityRequirement(securityRequirements);
	
	options.OperationFilter<MethodNeedsAuthorizationFilter>();
});

builder.Services.AddDbContext<HertejDbContext>(dbcob => dbcob.UseNpgsql(dbConnString));

builder.Services.Configure<RatingOptions>(builder.Configuration.GetRequiredSection("Rating"));
builder.Services.Configure<LocalFileStorage.Options>(builder.Configuration.GetRequiredSection("Storage"));

builder.Services.AddSingleton<FileStorage, LocalFileStorage>();
builder.Services.AddSingleton<CompleteRatingsJob>();

builder.Services.AddScheduler(ctx => {
	ctx.AddJob<CompleteRatingsJob>(configure: so => so.CronSchedule = "0 * * * *");
	//ctx.AddJob<CrawlImagesJob>(configure: so => so.CronSchedule = "0 * * * *");
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
		policy.RequireAuthenticatedUser();
		policy.RequireRole(authConfig.UploadRole);
	});

	options.AddPolicy("Rate", policy => {
		policy.RequireAuthenticatedUser();
		policy.RequireRole(authConfig.RateRole);
	});

	options.AddPolicy("Admin", policy => {
		policy.RequireAuthenticatedUser();
		policy.RequireRole(authConfig.AdminRole);
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
