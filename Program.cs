using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using System.Text.Json.Serialization;
using DotNetEnv;
using Scalar.AspNetCore;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
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

// RabbitMQ
builder.Services.AddSingleton<RabbitMqConnection>();
builder.Services.AddSingleton<IEventBus, RabbitMqEventBus>();
builder.Services.AddHostedService<AuditConsumer>();

// Middlewares
builder.Services.AddErrorHandling();
builder.Services.AddJwtAuthentication();
builder.Services.AddRequestLogging();
builder.Services.AddRateLimiting();
builder.Services.AddInputValidation();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseErrorHandling();
app.UseRateLimiter();
app.UseRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();