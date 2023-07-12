using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using QPlay.Common.Entities.Interfaces;
using QPlay.Common.Repositories.Interfaces;
using QPlay.Common.Settings;

namespace QPlay.Common.MongoDB;

/// <summary>
/// This class provides extension methods to register MongoDB services and repositories into IServiceCollection.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Adds the MongoDB related settings and services to the IServiceCollection.
    /// It registers custom serializers for Guid and DateTimeOffset types.
    /// Also, it adds a singleton instance of IMongoDatabase to the services.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddMongo(this IServiceCollection services)
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
        BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

        services.AddSingleton(serviceProvider =>
        {
            IConfiguration configuration = serviceProvider.GetService<IConfiguration>();
            ServiceSettings serviceSettings = configuration
                .GetSection(nameof(ServiceSettings))
                .Get<ServiceSettings>();
            MongoDBSettings mongoDbSettings = configuration
                .GetSection(nameof(MongoDBSettings))
                .Get<MongoDBSettings>();
            MongoClient mongoClient = new(mongoDbSettings.ConnectionString);
            return mongoClient.GetDatabase(serviceSettings.ServiceName);
        });

        return services;
    }

    /// <summary>
    /// Adds a specific MongoDB repository for the given entity type T to the IServiceCollection.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="services">The IServiceCollection to add the repository to.</param>
    /// <param name="collectionName">The name of the collection in the MongoDB database.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddMongoRepository<T>(
        this IServiceCollection services,
        string collectionName
    )
        where T : IEntity
    {
        services.AddSingleton<IRepository<T>>(serviceProvider =>
        {
            IMongoDatabase mongoDatabase = serviceProvider.GetService<IMongoDatabase>();
            return new MongoRepository<T>(mongoDatabase, collectionName);
        });

        return services;
    }
}
