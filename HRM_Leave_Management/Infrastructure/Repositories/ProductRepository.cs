using System.Linq.Expressions;
using Domain.Abstractions;
using Domain.Categories;
using Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    internal sealed class ProductRepository : Repository<Product, ProductId>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext dbContext)
            : base(dbContext)
        {
        }

        public async Task<Product?> GetByIdAsync(ProductId id)
        {
            return await DbContext.Set<Product>()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public IQueryable<Product> GetProductsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<Product>?> GetProductsAsync(CategoryId categoryId)
        {
            throw new NotImplementedException();
        }
    }
}