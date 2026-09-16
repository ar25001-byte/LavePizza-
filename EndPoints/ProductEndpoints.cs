using VeraPizza.DTOs;
using VeraPizza.Services;

namespace VeraPizza.EndPoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
                       .WithTags("Products");

        // GET /api/products
        group.MapGet("/", async (IProductServices productService) =>
        {
            var products = await productService.GetAllProductsAsync();
            return Results.Ok(products);
        })
        .WithName("GetAllProducts")
        .WithSummary("Obtener lista de productos")
        .WithDescription("Retorna el catálogo completo de productos disponibles en la pizzería.")
        .Produces<IEnumerable<ProductResponse>>(StatusCodes.Status200OK)
        .WithOpenApi();

        // GET /api/products/{id}
        group.MapGet("/{id:int}", async (int id, IProductServices productService) =>
        {
            var product = await productService.GetProductByIdAsync(id);
            return product is not null ? Results.Ok(product) : Results.NotFound(new { message = $"Producto con ID {id} no encontrado." });
        })
        .WithName("GetProductById")
        .WithSummary("Obtener producto por ID")
        .Produces<ProductResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithOpenApi();

        // POST /api/products (Protegido con JWT)
        group.MapPost("/", async (ProductRequest request, IProductServices productService) =>
        {
            var createdProduct = await productService.CreateProductAsync(request);
            return Results.Created($"/api/products/{createdProduct.Id}", createdProduct);
        })
        .WithName("CreateProduct")
        .WithSummary("Crear nuevo producto")
        .Produces<ProductResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithOpenApi();

        // PUT /api/products/{id} (Protegido con JWT)
        group.MapPut("/{id:int}", async (int id, ProductRequest request, IProductServices productService) =>
        {
            var updatedProduct = await productService.UpdateProductAsync(id, request);
            return updatedProduct is not null ? Results.Ok(updatedProduct) : Results.NotFound(new { message = $"Producto con ID {id} no encontrado." });
        })
        .WithName("UpdateProduct")
        .WithSummary("Modificar producto existente")
        .Produces<ProductResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithOpenApi();

        // DELETE /api/products/{id} (Protegido con JWT)
        group.MapDelete("/{id:int}", async (int id, IProductServices productService) =>
        {
            var success = await productService.DeleteProductAsync(id);
            return success ? Results.NoContent() : Results.NotFound(new { message = $"Producto con ID {id} no encontrado." });
        })
        .WithName("DeleteProduct")
        .WithSummary("Eliminar producto")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireAuthorization()
        .WithOpenApi();
    }
}
