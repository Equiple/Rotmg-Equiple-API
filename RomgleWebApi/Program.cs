using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using RomgleWebApi.DAL;
using RomgleWebApi.Data.Settings;
using RomgleWebApi.IdentityValidators;
using RomgleWebApi.ModelBinding.ValueProviders.Factories;
using RomgleWebApi.Services;
using RomgleWebApi.Services.Implementations;
using RomgleWebApi.Services.ServiceCollectionExtensions;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

IConfiguration authorizationConfig = builder.Configuration.GetSection("TokenAuthorization");

TokenAuthorizationSettings authorizationSettings = new TokenAuthorizationSettings();
authorizationConfig.Bind(authorizationSettings);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = authorizationSettings.ValidateIssuer,
            ValidIssuer = authorizationSettings.Issuer,

            ValidateAudience = authorizationSettings.ValidateAudience,
            ValidAudience = authorizationSettings.Audience,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = authorizationSettings.GetSecurityKey()
        };
    });

builder.Services.AddCors();

builder.Services.AddControllers(options =>
{
    options.ValueProviderFactories.Add(new UserValueProviderFactory());
    options.ValueProviderFactories.Add(new CookieValueProviderFactory());
}).AddJsonOptions(options => 
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

ConventionPack mongoConventions = new ConventionPack();
mongoConventions.Add(new EnumRepresentationConvention(BsonType.String));
ConventionRegistry.Register("aMongo", mongoConventions, _ => true);

builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<RotmgleDatabaseSettings>(
    builder.Configuration.GetSection("RotmgleDatabase"));
builder.Services.Configure<TokenAuthorizationSettings>(authorizationConfig);

builder.Services.AddAuthenticationService(
    new SelfAuthenticationValidator());
builder.Services.AddSingleton<IDataCollectionProvider, DefaultMongoDataCollectionProvider>();
builder.Services.AddSingleton<IAccessTokenService, AccessTokenService>();
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
