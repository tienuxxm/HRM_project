using Application.Abstractions.Messaging;


namespace Application.Categories.GetOne
{
    public record GetCategoryQuery() : IQuery<CategoryResponse>
    {
        public required Guid Id { get; init; }
    }
}