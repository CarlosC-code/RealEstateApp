using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> AddAsync(T entity);
        Task<List<T>?> AddRangeAsync(List<T> entities);
        Task<T?> UpdateAsync(int id, T entity);
        Task DeleteAsync(int id);
        Task<List<T>> GetAllAsync();
        Task<List<T>> GetAllWithIncludeAsync(List<string> properties);
        Task<T?> GetByIdAsync(int id);
        IQueryable<T> GetAllQuery();
        IQueryable<T> GetAllQueryWithInclude(List<string> properties);
    }
}