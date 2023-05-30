using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using RotmgleWebApi.Authentication;
using RotmgleWebApi.ModelBinding;
using System.Text.Json.Serialization;
using RotmgleWebApi.Jobs;
using RotmgleWebApi.Dailies;
using RotmgleWebApi.Players;
using RotmgleWebApi.Games;
using RotmgleWebApi.Items;
using RotmgleWebApi.Complaints;
using RotmgleWebApi;
using Microsoft.Net.Http.Headers;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings());

builder.Services.AddHangfireServer();

const string authenticationScheme = "Bearer";
builder.Services.AddAuthentication(authenticationScheme)
    .AddScheme<TokenAuthenticationOptions, TokenAuthenticationHandler>(authenticationScheme, options =>
    {
        options.RefreshTokenRequestPath = "Authentication/RefreshAccessToken";
    });

builder.Services.AddAuthorization();

builder.Services.AddCors();

builder.Services.AddControllers(options =>
{
    options.ValueProviderFactories.Add(new UserValueProviderFactory());
    options.ValueProviderFactories.Add(new DeviceIdValueProviderFactory());
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.Converters.Add(new JsonStringTimeSpanConverter());
});

builder.Services.AddDeviceIdProviders("default")
    .Add<UserAgentDeviceIdProvider>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<RotmgleDatabaseSettings>(
    builder.Configuration.GetSection("RotmgleDatabase"));
builder.Services.Configure<TokenAuthorizationSettings>(
    builder.Configuration.GetSection("TokenAuthorization"));

builder.Services.AddAuthenticationService()
    .AddValidator<GoogleAuthenticationValidator>();
builder.Services.AddSingleton<IAccessTokenService, JWTService>();
builder.Services.AddSingleton<IItemService, ItemService>();
builder.Services.AddSingleton<IDailyService, DailyService>();
builder.Services.AddSingleton<IPlayerService, PlayerService>();
builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddSingleton<IJobService, JobService>();
builder.Services.AddSingleton<IComplaintService, ComplaintService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        In = ParameterLocation.Header,
        Name = HeaderNames.Authorization,
        Scheme = "Bearer",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        },
    });
    options.OperationFilter<CustomBindingSourceSwaggerFilter>();
    options.MapType<TimeSpan>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "time",
    });
});

StaticRegistrationHelper.ProdOnce("Startup", () =>
{
    //mongo
    ConventionPack mongoConventions = new();
    mongoConventions.Add(new EnumRepresentationConvention(BsonType.String));
    ConventionRegistry.Register("MongoDbConvention", mongoConventions, _ => true);
    BsonClassMapInitializer.Initialize();

    //hangfire
    var migrationOptions = new MongoMigrationOptions
    {
        MigrationStrategy = new MigrateMongoMigrationStrategy(),
        BackupStrategy = new CollectionMongoBackupStrategy(),
    };
    GlobalConfiguration.Configuration.UseMongoStorage(
        builder.Configuration.GetSection("Hangfire")["ConnectionString"],
        new MongoStorageOptions
        {
            MigrationOptions = migrationOptions,
            CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.TailNotificationsCollection
        });
    RecurringJobInitializer.Initialize();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    app.UseDeveloperExceptionPage();

    //app.UseHangfireDashboard();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

//$"Exception at {nameof()} method, {GetType().Name} class: Exception message [{}].\n"