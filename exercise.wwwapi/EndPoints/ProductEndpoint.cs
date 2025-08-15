using exercise.wwwapi.DTOs;
using exercise.wwwapi.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace exercise.wwwapi.EndPoints
{
    public static class ProductEndpoint
    {
        public static void ConfigureProduct(this WebApplication app)
        {
            var products = app.MapGroup("products");

            products.MapGet("/", GetProducts);
            products.MapGet("/{id}", GetProductById);
            products.MapPost("/", AddProduct);
            products.MapPut("/{id}", UpdateProduct);
            products.MapDelete("/{id}", DeleteProduct);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        public static async Task<IResult> GetProducts(IRepository repository)
        {
            return TypedResults.Ok(await repository.GetProductsAsync());
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public static async Task<IResult> GetProductById(IRepository repository, int id)
        {
            var target = await repository.GetByIdAsync(id);

            if (target is null)
                return TypedResults.NotFound();

            return TypedResults.Ok(target);
        }

        public static async Task<IResult> AddProduct(IRepository repository, ProductPost model)
        {
            var addedEntity = await repository.AddAsync(model);

            return TypedResults.Ok(addedEntity);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public static async Task<IResult> UpdateProduct(IRepository repository, int id, ProductPut model)
        {
            var entity = await repository.GetByIdAsync(id);

            if (entity is null)
                return TypedResults.NotFound();

            if (model.Name is not null) entity.Name = model.Name;
            if (model.Category is not null) entity.Category = model.Category;
            if (model.Price is not null) entity.Price = model.Price.GetValueOrDefault();

            var updatedEntity = await repository.UpdateAsync(id, entity);

            return TypedResults.Ok(updatedEntity);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public static async Task<IResult> DeleteProduct(IRepository repository, int id)
        {
            var deletedEntity = await repository.DeleteAsync(id);

            if (deletedEntity is null)
                return TypedResults.NotFound();

            return TypedResults.Ok(deletedEntity);
        }
    }
}
