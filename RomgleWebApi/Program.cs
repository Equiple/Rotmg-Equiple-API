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
using RotmgleWebApi.AuthenticationImplementation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings());

builder.Services.AddHangfireServer();

builder.Services.AddAuthentication(TokenAuthenticationDefaults.Scheme)
    .AddToken<IdentityProvider, UserService, SessionService>(options =>
    {
        builder.Configuration.Bind("TokenAuthentication", options);
    });
builder.Services.AddMemoryCache();
builder.Services.AddAuthenticationValidators<IdentityProvider>()
    .AddValidator<RealmeyeAuthenticationValidator>();
builder.Services.AddDeviceIdProviders("default")
    .AddUserAgentDeviceIdProvider();
builder.Services.Configure<TokenAuthenticationStorageOptions>(
    builder.Configuration.GetSection("TokenAuthenticationStorage"));
builder.Services.Configure<RealmeyeAuthenticationOptions>(
    builder.Configuration.GetSection("RealmeyeAuthentication"));

builder.Services.AddAuthorization();

builder.Services.AddCors();

builder.Services.AddControllers(options =>
{
    options.ValueProviderFactories.Add(new UserValueProviderFactory());
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.Converters.Add(new JsonStringTimeSpanConverter());
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<RotmgleDatabaseOptions>(
    builder.Configuration.GetSection("RotmgleDatabase"));

builder.Services.AddSingleton<IItemService, ItemService>();
builder.Services.AddSingleton<IDailyService, DailyService>();
builder.Services.AddSingleton<IPlayerService, PlayerService>();
builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddSingleton<IJobService, JobService>();
builder.Services.AddSingleton<IComplaintService, ComplaintService>();

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
    mongoConventions.Add(new NamedExtraElementsMemberConvention(nameof(IExtraElements.ExtraElements)));
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

if (app.Environment.IsDevelopment())
{
    app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    app.UseDeveloperExceptionPage();

    //app.UseHangfireDashboard();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

//$"Exception at {nameof()} method, {GetType().Name} class: Exception message [{}].\n"