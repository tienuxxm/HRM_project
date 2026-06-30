namespace Application.Categories.GetOne
{
    public sealed class CategoryResponse
    {
        public Guid Id { get; init; }

        public string CategoryName { get; init; }
        public string Description { get; init; }
        public int? Index { get; init; }
    }
}