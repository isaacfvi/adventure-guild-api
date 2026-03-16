using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});

// MongoDB Config
var mongoConnection = Environment.GetEnvironmentVariable("MONGO_CONNECTION");
var mongoDatabase = Environment.GetEnvironmentVariable("MONGO_DATABASE");

builder.Services.AddSingleton<IMongoClient>(s =>
    new MongoClient(mongoConnection));

builder.Services.AddSingleton(s =>
{
    var client = s.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDatabase);
});

// GUID Serialization Config
MongoDB.Bson.Serialization.BsonSerializer.RegisterSerializer(
    new GuidSerializer(GuidRepresentation.Standard)
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();