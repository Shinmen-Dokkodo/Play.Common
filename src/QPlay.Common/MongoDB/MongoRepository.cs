using MongoDB.Driver;
using QPlay.Common.Entities.Interfaces;
using QPlay.Common.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace QPlay.Common.MongoDB;

/// <summary>
/// Repository implementation for MongoDB, for a generic type T where T is an entity.
/// </summary>
public class MongoRepository<T> : IRepository<T> where T : IEntity
{
    private readonly IMongoCollection<T> mongoCollection;
    private readonly FilterDefinitionBuilder<T> filterDefinitionBuilder = Builders<T>.Filter;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoRepository{T}"/> class.
    /// </summary>
    /// <param name="mongoDatabase">The MongoDB database.</param>
    /// <param name="collectionName">The name of the collection.</param>
    public MongoRepository(IMongoDatabase mongoDatabase, string collectionName)
    {
        mongoCollection = mongoDatabase.GetCollection<T>(collectionName);
    }

    /// <summary>
    /// Gets all entities asynchronously.
    /// </summary>
    /// <returns>A collection of all entities.</returns>
    public async Task<IReadOnlyCollection<T>> GetAllAsync()
    {
        return await mongoCollection.Find(filterDefinitionBuilder.Empty).ToListAsync();
    }

    /// <summary>
    /// Gets all entities that match the provided filter asynchronously.
    /// </summary>
    /// <param name="filter">The filter expression.</param>
    /// <returns>A collection of entities that match the filter.</returns>
    public async Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> filter)
    {
        return await mongoCollection.Find(filter).ToListAsync();
    }

    /// <summary>
    /// Gets a single entity by its Id asynchronously.
    /// </summary>
    /// <param name="id">The Id of the entity.</param>
    /// <returns>The entity if found; otherwise null.</returns>
    public async Task<T> GetAsync(Guid id)
    {
        FilterDefinition<T> filterDefinition = filterDefinitionBuilder.Eq(entity => entity.Id, id);
        return await mongoCollection.Find(filterDefinition).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets a single entity that matches the provided filter asynchronously.
    /// </summary>
    /// <param name="filter">The filter expression.</param>
    /// <returns>The entity if found; otherwise null.</returns>
    public async Task<T> GetAsync(Expression<Func<T, bool>> filter)
    {
        return await mongoCollection.Find(filter).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Creates a new entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task CreateAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        await mongoCollection.InsertOneAsync(entity);
    }

    /// <summary>
    /// Updates an existing entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task UpdateAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        FilterDefinition<T> filterDefinition = filterDefinitionBuilder.Eq(existingEntity => existingEntity.Id, entity.Id);
        await mongoCollection.ReplaceOneAsync(filterDefinition, entity);
    }

    /// <summary>
    /// Removes an entity by its Id asynchronously.
    /// </summary>
    /// <param name="id">The Id of the entity to remove.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task RemoveAsync(Guid id)
    {
        FilterDefinition<T> filterDefinition = filterDefinitionBuilder.Eq(entity => entity.Id, id);
        await mongoCollection.DeleteOneAsync(filterDefinition);
    }
}
