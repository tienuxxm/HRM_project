using Domain.Abstractions;

namespace Domain.Categories;

public static class CategoryErrors
{
    public static Error NotFound = new(
        "Category.Found",
        "The category with the specified identifier was not found");

    public static Error CategoryExisted = new("Category.Existed", "The category existed");
}