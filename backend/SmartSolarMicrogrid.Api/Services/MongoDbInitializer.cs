using MongoDB.Bson;
using MongoDB.Driver;
using SmartSolarMicrogrid.Api.Repositories;

namespace SmartSolarMicrogrid.Api.Services;

public sealed class MongoDbInitializer(
    IMongoDatabase database,
    IUserDetailsRepository userDetailsRepository,
    ILogger<MongoDbInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Ping the selected database, then create the required indexes before serving requests.
        await database.RunCommandAsync<BsonDocument>(
            new BsonDocument("ping", 1),
            cancellationToken: cancellationToken);

        await userDetailsRepository.EnsureIndexesAsync(cancellationToken);

        logger.LogInformation(
            "MongoDB connectivity verified and UserDetails indexes initialized for database {DatabaseName}.",
            database.DatabaseNamespace.DatabaseName);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
