using Application.Abstractions.Messaging;
using Application.Products.GetOne;

namespace Application.Products.GetAll;

public record GetAllProductCommand(bool allowDelivery = false) : ICommand<List<ProductResponse>>;