using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Application.Interfaces.Repositories;
using backend.Infrastructure.Data;
using backend.Infrastructure.Logging;


namespace backend.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation for CRUD operations.
/// </summary>

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly AppDbContext _dbContext;
    private readonly DbSet<T> _dbSet;
    private readonly IEntityFileLogger _entityLogger;

    // Constructor
    public GenericRepository(AppDbContext dbContext, IEntityFileLogger entityLogger)
	{
		_dbContext = dbContext;
		_dbSet = _dbContext.Set<T>();
        _entityLogger = entityLogger;
	}
    
    public async Task<T> GetById(int id)
    {
        try
        {
            var entity = await _dbSet.FindAsync(id);

            if (entity == null)
            {
                await _entityLogger.LogAsync(
                    "GetByIdNotFound", 
                    new { 
                        EntityType = typeof(T).Name,
                        Id = id,
                        Reason = "Entity not found",
                        Timestamp = DateTime.UtcNow
                        }, 
                    LogCategories.SystemLevel.Database);

                throw new KeyNotFoundException($"Entity of type {typeof(T).Name} with id {id} was not found.");
            }


            return entity;
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetByIdError", 
                new { 
                    EntityType = typeof(T).Name,
                    Id = id,
                    Reason = ex.Message,
                    Timestamp = DateTime.UtcNow
                    }, 
                LogCategories.SystemLevel.Database);
            throw;
        }
    }

    public async Task<IEnumerable<T>> GetAll()
    {
        try {
            
            // get all of that specific entity type from the db set
            return await _dbSet.AsNoTracking().ToListAsync();

        } catch (Exception ex) {
            await _entityLogger.LogAsync(
                "GetAllError", 
                new { 
                    EntityType = typeof(T).Name,
                    Reason = ex.Message,
                    Timestamp = DateTime.UtcNow
                    }, 
                LogCategories.SystemLevel.Database);
            throw;
        }
    }


    public async Task<T> Create(T entity)
    {

        try {

            if ( entity == null) {
                throw new ArgumentNullException(nameof(entity), $"Cannot create null entity of type {typeof(T).Name}");
            }

            // create the entity
            await _dbSet.AddAsync(entity);

            // save the changes to the database
            await _dbContext.SaveChangesAsync();
            
            await _entityLogger.LogAsync(
                "CreateSuccess", 
                new { 
                    EntityType = typeof(T).Name,
                    Entity = entity,
                    Timestamp = DateTime.UtcNow
                    }, 
                LogCategories.SystemLevel.Database);

            // return the entity
            return entity;
            
        }  catch (DbUpdateConcurrencyException ex) {
            // catch specific to the database update errors.
            throw new InvalidOperationException($"Failed to create entity of type {typeof(T).Name}. Concurrency conflict detected.", ex);

        } catch (DbUpdateException ex) {
            // catch specific to the database update errors.
            throw new InvalidOperationException($"Failed to create entity of type {typeof(T).Name}. Database constraint violation.", ex);
        
        } catch (Exception ex) {
            await _entityLogger.LogAsync(
                "CreateError", 
                new { 
                    EntityType = typeof(T).Name,
                    Entity = entity,
                    Reason = ex.Message,
                    Timestamp = DateTime.UtcNow
                    }, 
                LogCategories.SystemLevel.Database);
            throw;
        }
    }

    public async Task<T> Update(T entity)
    {
        try {

            // check if the entity is null
            if ( entity == null) {
                throw new ArgumentNullException(nameof(entity), $"Cannot update null entity of type {typeof(T).Name}");
            }

            // attach the entity to the db set
            _dbSet.Attach(entity);
            // set the state to modified
            _dbContext.Entry(entity).State = EntityState.Modified;

            // save the changes to the database
            await _dbContext.SaveChangesAsync();
            
            await _entityLogger.LogAsync(
                "UpdateSuccess", 
                new { 
                    EntityType = typeof(T).Name,
                    Entity = entity,
                    Timestamp = DateTime.UtcNow
                    }, 
                LogCategories.SystemLevel.Database);
            // return the entity
            return entity;

        } catch (DbUpdateConcurrencyException ex) {
            // catch specific to the database update errors.
            throw new InvalidOperationException($"Failed to update entity of type {typeof(T).Name}. Concurrency conflict detected.", ex);

        } catch (DbUpdateException ex) {
            // catch specific to the database update errors.
            throw new InvalidOperationException($"Failed to update entity of type {typeof(T).Name}. Database constraint violation.", ex);

        } catch (Exception ex) {
            await _entityLogger.LogAsync(
                "UpdateError", 
                new { 
                    EntityType = typeof(T).Name,
                    Entity = entity,
                    Reason = ex.Message,
                    Timestamp = DateTime.UtcNow
                    }, 
                LogCategories.SystemLevel.Database);
            throw;
        }

    }


    // delete entity
    public async Task<T> Delete(int id)
    {
        try {

            // get the entity by id.
            var entity = await _dbSet.FindAsync(id);

            // if entity is not found, throw error
            if (entity == null) {
                await _entityLogger.LogAsync(
                    "DeleteNotFound", 
                    new { 
                        EntityType = typeof(T).Name,
                        Id = id,
                        Reason = "Entity not found",
                        Timestamp = DateTime.UtcNow
                        }, 
                    LogCategories.SystemLevel.Database);
                    
                throw new KeyNotFoundException($"Entity of type {typeof(T).Name} with id {id} was not found.");
            }

            // else remove the entity
            _dbSet.Remove(entity);

            // save changes
            await _dbContext.SaveChangesAsync();
            
            await _entityLogger.LogAsync(
                "DeleteSuccess", 
                new { 
                    EntityType = typeof(T).Name,
                    Id = id,
                    Timestamp = DateTime.UtcNow
                    }, 
                LogCategories.SystemLevel.Database);

            // return the entity
            return entity;

        } catch (KeyNotFoundException) {
            throw;
        } catch (DbUpdateException ex) {
            // catch specific to the database update errors.
            throw new InvalidOperationException($"Failed to delete entity of type {typeof(T).Name} with id {id}. Database constraint violation.", ex);

        } catch (Exception ex) {
            // catch general exceptions.
            await _entityLogger.LogAsync(
                "DeleteError", 
                new { 
                    EntityType = typeof(T).Name,
                    Id = id,
                    Reason = ex.Message,
                    Timestamp = DateTime.UtcNow
                    }, 
                LogCategories.SystemLevel.Database);
            throw;
        }
    }

    // get by property gets the entitys property
    // such as name, id or etc.

    // help from GPT 5. In reducing the duplicates from the codebase. 
    public async Task<T?> GetByProperty<TProperty>(Expression<Func<T, TProperty>> propertySelector, TProperty value)
    {
        try {

            // parameters for the expression
            var param = Expression.Parameter(typeof(T), "entity");

            //property
            var prop = Expression.Property(
                param, ((MemberExpression)propertySelector.Body).Member.Name
            );

            // constant
            var constant = Expression.Constant(value);
            
            // equal to the constant
            var equal = Expression.Equal(prop, constant);

            //lambda expression equals to the param
            var lambda = Expression.Lambda<Func<T, bool>>(equal, param);

            // finally return the lambda expression
            return await _dbSet.FirstOrDefaultAsync(lambda);

        } catch (Exception ex) {
            await _entityLogger.LogAsync(
                "GetByPropertyError", 
                new { 
                    EntityType = typeof(T).Name,
                    Reason = ex.Message,
                    Timestamp = DateTime.UtcNow
                    }, 
                LogCategories.SystemLevel.Database);
            throw;
        }
    }

    // get all by property gets all the entities properties
    // such as name, id or etc.
    public async Task<IEnumerable<T>> GetAllByProperty<TProperty>(Expression<Func<T, TProperty>> propertySelector, TProperty value)
    {
        try {
            // same process as the get by property

            // params
            var param = Expression.Parameter(typeof(T), "entity");

            // property
            var prop = Expression.Property(param, ((MemberExpression)propertySelector.Body).Member.Name);

            // constant
            var constant = Expression.Constant(value);

            // equal to the constant
            var equal = Expression.Equal(prop, constant);

            // lambda expression equals to the param
            var lambda = Expression.Lambda<Func<T, bool>>(equal, param);

            // finally return the lambda expression that gets all the entities by the property
            return await _dbSet.Where(lambda).ToListAsync();

        } catch (Exception ex) {
            await _entityLogger.LogAsync(
                "GetAllByPropertyError", 
                new { 
                    EntityType = typeof(T).Name,
                    Property = propertySelector.Body.ToString(),
                    Reason = ex.Message,
                    Timestamp = DateTime.UtcNow
                    }, 
                LogCategories.SystemLevel.Database);
            throw;
        }
    }


    // get property value by the entity's Id
    public async Task<TProperty?> GetPropertyValue<TProperty>(int id, Expression<Func<T, TProperty>> propertySelector)
    {
        try {

            // get the entity by id and select the property we want to get  
            return await _dbSet.Where(e => EF.Property<int>(e, "Id") == id)
                        .Select(propertySelector)
                        .FirstOrDefaultAsync();

        } catch (Exception ex) {
            await _entityLogger.LogAsync(
                "GetPropertyValueError", 
                new { 
                    EntityType = typeof(T).Name,
                    Id = id,
                    Reason = ex.Message,
                    Timestamp = DateTime.UtcNow
                    }, 
                LogCategories.SystemLevel.Database);
            throw new Exception($"Error retrieving property value for entity of type {typeof(T).Name} with id {id}: {ex.Message}");
        }
    }
}   