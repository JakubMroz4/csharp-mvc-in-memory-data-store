using exercise.wwwapi.DTOs;
using exercise.wwwapi.Models;

namespace exercise.wwwapi.Repository
{
    public interface IRepository
    {
        Task<List<Product>> GetProductsAsync();
        Task<Product> GetByIdAsync(int id);
        Task<Product> AddAsync(ProductPost product);
        Task<Product> DeleteAsync(int id);
        Task<Product> UpdateAsync(int id, Product model);

        Task<Product> NameExistsAsync(string name);
    }
}
