using exercise.wwwapi.DTOs;
using exercise.wwwapi.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Buffers.Text;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace exercise.wwwapi.EndPoints
{
    public static class ProductEndpoint
    {
        private static readonly string subdomain = "products";
        public static void ConfigureProduct(this WebApplication app)
        {
            var products = app.MapGroup(subdomain);

            products.MapGet("/", GetProducts);
            products.MapGet("/{id}", GetProductById);
            products.MapPost("/", AddProduct).Accepts<ProductPost>("application/json");
            products.MapPut("/{id}", UpdateProduct).Accepts<ProductPut>("application/json");
            products.MapDelete("/{id}", DeleteProduct);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public static async Task<IResult> AddProduct(HttpRequest request, HttpContext context, IRepository repository)
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

            var url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}/{addedEntity.Id}";
            return TypedResults.Created(url);
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public static async Task<IResult> UpdateProduct(HttpRequest request, HttpContext context, IRepository repository, int id)
        {
            ProductPut? model;
            try
            {
                model = await request.ReadFromJsonAsync<ProductPut>();
            }
            catch (JsonException ex)
            {
                return Results.BadRequest($"Price must be a number, something else was provided");
            }

            var existingEntity = await repository.NameExistsAsync(model.Name);
            if (existingEntity is not null && existingEntity.Id != id)
            {
                return TypedResults.BadRequest("Product with provided name already exists.");
            }

            var entity = await repository.GetByIdAsync(id);

            if (entity is null)
                return TypedResults.NotFound();

            if (model.Name is not null) entity.Name = model.Name;
            if (model.Category is not null) entity.Category = model.Category;
            if (model.Price is not null) entity.Price = model.Price.GetValueOrDefault();

            var updatedEntity = await repository.UpdateAsync(id, entity);

            var url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}";
            return TypedResults.Created(url);
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
