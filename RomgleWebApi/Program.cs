using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using RomgleWebApi.Data;
using RomgleWebApi.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors();

builder.Services.AddControllers().AddJsonOptions(options => 
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

ConventionPack mongoConventions = new ConventionPack();
mongoConventions.Add(new EnumRepresentationConvention(BsonType.String));
ConventionRegistry.Register("aMongo", mongoConventions, x => true);

builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<RotmgleDatabaseSettings>(
    builder.Configuration.GetSection("RotmgleDatabase"));

builder.Services.AddSingleton<ItemsService>();
builder.Services.AddSingleton<DailiesService>();
builder.Services.AddSingleton<PlayersService>();
builder.Services.AddSingleton<GameService>();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
