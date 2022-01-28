using Ardalis.GuardClauses;
using ECommerce.Services.Catalogs.Categories.Exceptions.Application;

namespace ECommerce.Services.Catalogs.Categories;

public static class GuardExtensions
{
    public static void NullCategory(this IGuardClause guardClause, Category? category, long categoryId)
    {
        if (category == null)
            throw new CategoryNotFoundException(categoryId);
    }

    public static void ExistsCategory(this IGuardClause guardClause, bool exists, long categoryId)
    {
        if (exists == false)
            throw new CategoryNotFoundException(categoryId);
    }
}