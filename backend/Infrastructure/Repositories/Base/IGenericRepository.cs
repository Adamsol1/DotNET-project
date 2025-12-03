using System.Linq.Expressions;

namespace backend.Infrastructure.Repositories.Base;

/*
    Generic Repository interface,
    Has all the Basic crud operations that all repositories share

*/

public interface IGenericRepository<T> where T : class
{

    // get By Id Asyncrounusly
    Task<T> GetById(int id);

    // get All Asyncrounusly
    Task<IEnumerable<T>> GetAll();

    // create Async
    Task<T> Create(T entity);

    Task<T> Update(T entity);

    Task<T> Delete(int id);

    // get by property
    Task<T?> GetByProperty<TProperty>(Expression<Func<T, TProperty>> propertySelector, TProperty value);

    // get all by property
    Task<IEnumerable<T>> GetAllByProperty<TProperty>(Expression<Func<T, TProperty>> propertySelector, TProperty value);

    // get specific property value
    Task<TProperty?> GetPropertyValue<TProperty>(int id, Expression<Func<T, TProperty>> propertySelector);
}