using Domain.Abstractions;

namespace Domain.FreeServices;

public static class FeeServicesError
{
    public static Error NotFound => new("FeeService.NotFound", "The Fee service with the specified identifier was not found");
}