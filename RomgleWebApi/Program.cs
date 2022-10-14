using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using RomgleWebApi.Authentication.AuthenticationHandlers;
using RomgleWebApi.Authentication.AuthenticationValidators;
using RomgleWebApi.Authentication.Options;
using RomgleWebApi.Authorization.Handlers;
using RomgleWebApi.Authorization.Requirements;
using RomgleWebApi.DAL;
using RomgleWebApi.Data.Models.BsonClassMaps;
using RomgleWebApi.Data.Settings;
using RomgleWebApi.ModelBinding.ValueProviders.Factories;
using RomgleWebApi.Services;
using RomgleWebApi.Services.Implementations;
using RomgleWebApi.Services.ServiceCollectionExtensions;
using RomgleWebApi.Utils;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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
    options.AddPolicy(PolicyNames.IgnoreExpiration, policyBuilder => policyBuilder
        .Combine(basePolicy));
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
});

StaticRegistrationHelper.Scope(() =>
{
    ConventionPack mongoConventions = new ConventionPack();
    mongoConventions.Add(new EnumRepresentationConvention(BsonType.String));
    ConventionRegistry.Register("aMongo", mongoConventions, _ => true);

    BsonClassMapInitializer.Initialize();
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<RotmgleDatabaseSettings>(
    builder.Configuration.GetSection("RotmgleDatabase"));
builder.Services.Configure<TokenAuthorizationSettings>(
    builder.Configuration.GetSection("TokenAuthorization"));

builder.Services.AddAuthenticationService(
    new SelfAuthenticationValidator());
builder.Services.AddSingleton<IDataCollectionProvider, DefaultMongoDataCollectionProvider>();
builder.Services.AddSingleton<IAccessTokenService, JWTService>();
builder.Services.AddSingleton<IItemsService, ItemsService>();
builder.Services.AddSingleton<IDailiesService, DailiesService>();
builder.Services.AddSingleton<IPlayersService, PlayersService>();
builder.Services.AddSingleton<IGameService, GameService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen(); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

//$"Exception at {nameof()} method, {GetType().Name} class: Exception message [{}].\n"