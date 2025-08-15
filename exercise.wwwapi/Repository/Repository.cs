using exercise.wwwapi.Data;
using exercise.wwwapi.DTOs;
using exercise.wwwapi.Models;
using Microsoft.EntityFrameworkCore;

namespace exercise.wwwapi.Repository
{
    public class Repository : IRepository
    {
        private DataContext _db;

        public Repository(DataContext db)
        {
            _db = db;
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            return await _db.Products.ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _db.Products.FindAsync(id);
        }

        public async Task<Product> AddAsync(ProductPost model)
        {
            var product = new Product();
            product.Name = model.Name;
            product.Category = model.Category;
            product.Price = model.Price;

            await _db.Products.AddAsync(product);
            await _db.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateAsync(int id, Product model)
        {
            _db.Products.Update(model);
            await _db.SaveChangesAsync();

            return model;

        }

        public async Task<Product> DeleteAsync(int id)
        {
            var toRemove = await _db.Products.FindAsync(id);

            if (toRemove is null)
                return toRemove;

            _db.Products.Remove(toRemove);
            await _db.SaveChangesAsync();
            return toRemove;
        }

        public async Task<Product> NameExistsAsync(string name)
        {
            var entity = await _db.Products.Where(p => p.Name == name).FirstOrDefaultAsync();
            return entity;
        }
    }
}
