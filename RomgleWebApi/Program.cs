using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Hangfire.Mongo.Migration.Strategies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using RomgleWebApi.Authentication.Handlers;
using RomgleWebApi.Authentication.Validators;
using RomgleWebApi.Authentication.Options;
using RomgleWebApi.Authorization.Handlers;
using RomgleWebApi.Authorization.Requirements;
using RomgleWebApi.DAL;
using RomgleWebApi.Data.Models.BsonClassMaps;
using RomgleWebApi.Data.Settings;
using RomgleWebApi.Json.Converters;
using RomgleWebApi.ModelBinding.ValueProviders.Factories;
using RomgleWebApi.Services;
using RomgleWebApi.Services.Implementations;
using RomgleWebApi.Services.ServiceCollectionExtensions;
using RomgleWebApi.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using RomgleWebApi.Jobs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings());

builder.Services.AddHangfireServer();

const string authenticationScheme = "AccessToken";

builder.Services.AddAuthentication(authenticationScheme)
    .AddScheme<TokenAuthenticationOptions, TokenAuthenticationHandler>(
        authenticationScheme,
        options =>
        {
            options.IgnoreExpiration = true;
        });

builder.Services.AddAuthorization(options =>
{
    AuthorizationPolicy basePolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(authenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .Combine(basePolicy)
        .AddRequirements(new ExpirationAuthorizationRequirement(JwtRegisteredClaimNames.Exp))
        .Build();
    options.AddPolicy(
        PolicyNames.IgnoreExpiration,
        policyBuilder => policyBuilder.Combine(basePolicy));
});
builder.Services.AddSingleton<IAuthorizationHandler, ExpirationAuthorizationHandler>();

builder.Services.AddCors();

builder.Services.AddControllers(options =>
{
    options.ValueProviderFactories.Add(new UserValueProviderFactory());
    options.ValueProviderFactories.Add(new CookieValueProviderFactory());
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.Converters.Add(new JsonStringTimeSpanConverter());
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<RotmgleDatabaseSettings>(
    builder.Configuration.GetSection("RotmgleDatabase"));
builder.Services.Configure<TokenAuthorizationSettings>(
    builder.Configuration.GetSection("TokenAuthorization"));

builder.Services.AddAuthenticationService(
    new SelfAuthenticationValidator(),
    new GoogleAuthenticationValidator());
builder.Services.AddSingleton<IDataCollectionProvider, DefaultMongoDataCollectionProvider>();
builder.Services.AddSingleton<IAccessTokenService, JWTService>();
builder.Services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddSingleton<IItemService, ItemService>();
builder.Services.AddSingleton<IDailyService, DailyService>();
builder.Services.AddSingleton<IPlayerService, PlayerService>();
builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddSingleton<IJobService, JobService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen(options =>
{
    options.MapType<TimeSpan>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "time"
    });
});

StaticRegistrationHelper.ProdOnce("Startup", () =>
{
    //mongo
    ConventionPack mongoConventions = new ConventionPack();
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

    app.UseHangfireDashboard();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

//$"Exception at {nameof()} method, {GetType().Name} class: Exception message [{}].\n"