using Microsoft.Extensions.Options;
using RomgleWebApi.Data;
using RomgleWebApi.Services;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;
using static RomgleWebApi.Data.RealmeyeScraper;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options => 
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//{
//    RealmeyeScraper realmeye = new RealmeyeScraper();
//    realmeye.Start();
//}

app.Run();
