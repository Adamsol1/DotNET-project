using System.Linq.Expressions;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories.Base;

/// <summary>
/// Generic repository implementation for CRUD operations.
/// </summary>

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly AppDbContext _dbContext;
    private readonly DbSet<T> _dbSet;

    // Constructor
    public GenericRepository(AppDbContext dbContext)
	{
		_dbContext = dbContext;
		_dbSet = _dbContext.Set<T>();
	}
    
    public async Task<T> GetById(int id)
    {
        // find the entity by id
        var entity = await _dbSet.FindAsync(id);

        // if no entity then error
        if (entity == null) throw new KeyNotFoundException($"Entity of type {typeof(T).Name} with id {id} was not found.");

        return entity;
    }

    public async Task<IEnumerable<T>> GetAll()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }


    public async Task<T> Create(T entity)
    {
        await _dbSet.AddAsync(entity);
		await _dbContext.SaveChangesAsync();
		return entity;
    }

    public async Task<T> Update(T entity)
    {
        // attach the entity to the db set
        _dbSet.Attach(entity);
        // set the state to modified
        _dbContext.Entry(entity).State = EntityState.Modified;

        // update the entity
        await _dbContext.SaveChangesAsync();
        
        return entity;
    }


    // delete entity
    public async Task<T> Delete(int id)
    {
        // find the entity by id
        var entity = await _dbSet.FindAsync(id);

        // if entity is not found, throw error
        if (entity == null) throw new KeyNotFoundException($"Entity of type {typeof(T).Name} with id {id} was not found.");

        // else remove the entity
        _dbSet.Remove(entity);

        // save changes
        await _dbContext.SaveChangesAsync();

        // return the entity
        return entity;
    }

    // get by property gets the entitys property
    // such as name, id or etc.

    // help from GPT 5. In reducing the duplicates from the codebase. 
    public async Task<T?> GetByProperty<TProperty>(Expression<Func<T, TProperty>> propertySelector, TProperty value)
    {
        //params
        var param = Expression.Parameter(typeof(T), "entity");
        //property
        var prop = Expression.Property(
            param, ((MemberExpression)propertySelector.Body).Member.Name
        );
        // constant
        var constant = Expression.Constant(value);
        
        var equal = Expression.Equal(prop, constant);
        //labda expression
        var lambda = Expression.Lambda<Func<T, bool>>(equal, param);

        // finally return the lambda expression
        return await _dbSet.FirstOrDefaultAsync(lambda);
    }

    // get all by property gets all the entities properties
    // such as name, id or etc.
    public async Task<IEnumerable<T>> GetAllByProperty<TProperty>(Expression<Func<T, TProperty>> propertySelector, TProperty value)
    {
        var param = Expression.Parameter(typeof(T), "entity");

        var prop = Expression.Property(param, ((MemberExpression)propertySelector.Body).Member.Name);
        var constant = Expression.Constant(value);
        var equal = Expression.Equal(prop, constant);
        var lambda = Expression.Lambda<Func<T, bool>>(equal, param);
    
        return await _dbSet.Where(lambda).ToListAsync();
    }

    // get property value by the entity's Id
    public async Task<TProperty?> GetPropertyValue<TProperty>(int id, Expression<Func<T, TProperty>> propertySelector)
    {
        return await _dbSet.Where(e => EF.Property<int>(e, "Id") == id)
                        .Select(propertySelector)
                        .FirstOrDefaultAsync();
    }
}