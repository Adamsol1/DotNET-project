using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Application.Interfaces.Repositories;
using backend.Infrastructure.Data;

namespace backend.Infrastructure.Repositories;

/* Notes to myself -Ah

Initally I underestimated how many methods that would be dupes.
so I will change up stuff. and move frequently used methods to be a generic.
such as
- GetByProperty -> which will get properties from entities 
such as names or what you pass it.

- GetAllByProperty -> which will get all properties from entities

- GetPropertyValue

- this way we can remove all the getCharacterNames, description etc, 
 and keep the codebase clean and more chaotic.


*/

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    // set the database context
    private readonly AppDbContext _dbContext;
    // set the db set
    private readonly DbSet<T> _dbSet;

    // constructor
    public GenericRepository(AppDbContext dbContext)
	{
		_dbContext = dbContext;
		_dbSet = _dbContext.Set<T>();
	}
    
    // get by Id

    public async Task<T> GetById(int id)
    {
        // get the entity by id
        var entity = await _dbSet.FindAsync(id);

        // if no entity then error
        if (entity == null) throw new KeyNotFoundException($"Entity of type {typeof(T).Name} with id {id} was not found.");

        // return the entity
        return entity;
    }

    // get all
    public async Task<IEnumerable<T>> GetAll()
    {
        // get all the entities
        return await _dbSet.AsNoTracking().ToListAsync();

    }

    // create new entity
    public async Task<T> Create(T entity)
    {
        await _dbSet.AddAsync(entity);
		await _dbContext.SaveChangesAsync();
		return entity;
    }

    // update entity
    public async Task<T> Update(T entity)
    {
        // attach the entity to the db set
        _dbSet.Attach(entity);
        // set the state to modified
        _dbContext.Entry(entity).State = EntityState.Modified;

        // update the entity
        await _dbContext.SaveChangesAsync();

        // return the entity ? or a message?
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