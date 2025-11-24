using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace backend.Application.Interfaces.Repositories;

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

    Task<T?> GetByProperty<TProperty>(Expression<Func<T, TProperty>> propertySelector, TProperty value);

    Task<IEnumerable<T>> GetAllByProperty<TProperty>(Expression<Func<T, TProperty>> propertySelector, TProperty value);

    Task<TProperty?> GetPropertyValue<TProperty>(int id, Expression<Func<T, TProperty>> propertySelector);
}