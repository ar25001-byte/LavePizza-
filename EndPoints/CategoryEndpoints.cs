using VeraPizza.DTOs;
using VeraPizza.Services;

namespace VeraPizza.EndPoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories")
                       .WithTags("Categories");

        // GET /api/categories - Obtener todas las categorías
        group.MapGet("/", async (ICategoryServices categoryService) =>
        {
            var categories = await categoryService.GetAllCategoriesAsync();
            return Results.Ok(categories);
        })
        .WithName("GetAllCategories")
        .WithSummary("Obtener lista de categorías")
        .Produces<IEnumerable<CategoryResponse>>(StatusCodes.Status200OK)
        .AllowAnonymous()
        .WithOpenApi();

        // GET /api/categories/{id} - Obtener categoría por ID
        group.MapGet("/{id:int}", async (int id, ICategoryServices categoryService) =>
        {
            var category = await categoryService.GetCategoryByIdAsync(id);
            return category is not null
                ? Results.Ok(category)
                : Results.NotFound(new { message = $"Categoría con ID {id} no encontrada." });
        })
        .WithName("GetCategoryById")
        .WithSummary("Obtener categoría por ID")
        .Produces<CategoryResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .AllowAnonymous()
        .WithOpenApi();

        // POST /api/categories - Crear categoría (Privado)
        group.MapPost("/", async (CategoryRequest request, ICategoryServices categoryService) =>
        {
            var category = await categoryService.CreateCategoryAsync(request);
            return Results.Created($"/api/categories/{category.Id}", category);
        })
        .WithName("CreateCategory")
        .WithSummary("Crear nueva categoría")
        .Produces<CategoryResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .RequireAuthorization("RepositoryMemberOnly")
        .WithOpenApi();

        // PUT /api/categories/{id} - Modificar categoría (Privado)
        group.MapPut("/{id:int}", async (int id, CategoryRequest request, ICategoryServices categoryService) =>
        {
            var category = await categoryService.UpdateCategoryAsync(id, request);
            return category is not null
                ? Results.Ok(category)
                : Results.NotFound(new { message = $"Categoría con ID {id} no encontrada." });
        })
        .WithName("UpdateCategory")
        .WithSummary("Actualizar categoría")
        .Produces<CategoryResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .RequireAuthorization("RepositoryMemberOnly")
        .WithOpenApi();

        // DELETE /api/categories/{id} - Eliminar categoría (Privado)
        group.MapDelete("/{id:int}", async (int id, ICategoryServices categoryService) =>
        {
            var success = await categoryService.DeleteCategoryAsync(id);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { message = $"Categoría con ID {id} no encontrada." });
        })
        .WithName("DeleteCategory")
        .WithSummary("Eliminar categoría")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .RequireAuthorization("RepositoryMemberOnly")
        .WithOpenApi();
    }
}
