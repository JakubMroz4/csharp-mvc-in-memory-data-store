using exercise.wwwapi.DTOs;
using exercise.wwwapi.Repository;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
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
            products.MapPost("/", AddProduct).Accepts<ProductPost>("application/json");
            products.MapPut("/{id}", UpdateProduct);
            products.MapDelete("/{id}", DeleteProduct);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        public static async Task<IResult> GetProducts(IRepository repository, string? category)
        {
            if (category is null)
            {
                return TypedResults.Ok(await repository.GetProductsAsync());
            }

            var results = await repository.GetProductsByCategoryAsync(category);

            if (results.Count() == 0)
            {
                return TypedResults.NotFound("No Product of the provided category was found");
            }

            return TypedResults.Ok(results);
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

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public static async Task<IResult> AddProduct(HttpRequest request, IRepository repository)
        {
            ProductPost? model;
            try
            {
                model = await request.ReadFromJsonAsync<ProductPost>();
            }
            catch (JsonException ex)
            {
                return Results.BadRequest($"Price must be a number, something else was provided");
            }


            var existingEntity = await repository.NameExistsAsync(model.Name);
            if (existingEntity is not null)
            {
                return TypedResults.BadRequest("Product with provided name already exists.");
            }

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
