using Domain.Deliveries;

namespace Infrastructure.Repositories
{
    internal sealed class DeliveryRepository : Repository<Delivery, DeliveryId>, IDeliveryRepository
    {
        public DeliveryRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}