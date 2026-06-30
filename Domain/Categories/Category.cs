using Domain.Abstractions;
using Domain.Categories.Events;
using Domain.Shared;

namespace Domain.Categories;

public class Category : Entity<CategoryId>
{
    private Category(CategoryId
        id, CategoryName categoryName, Description description, DateTime createdDate, int? index)
    {
        Id = id;
        CategoryName = categoryName;
        Description = description;
        CreatedDate = createdDate;
        Index = index;
        IsDeleted = false;
    }

    private Category()
    {
    }

    public CategoryName CategoryName { get; private set; }
    public Description Description { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public int? Index { get; private set; }
    public bool? IsDeleted { get; private set; }

    public static Category Create(CategoryName categoryName, Description description, DateTime createdDate, int? index)
    {
        var category = new Category(CategoryId.New(), categoryName, description, createdDate, index);
        category.RaiseDomainEvent(new CategoryCreatedDomainEvent(category.Id));
        return category;
    }

    public void Update(CategoryName name, Description description, int? index)
    {
        CategoryName = name;
        Description = description;
        Index = index;
    }
}