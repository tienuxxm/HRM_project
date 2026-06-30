using Domain.Abstractions;

namespace Domain.Positions;

public static class PositionErrors
{
    public static Error NotFound = new(
        "Position.NotFound",
        "The position with the specified identifier was not found");

    public static Error PositionExisted = new(
        "Position.Existed",
        "A position with the same code already exists");

    public static Error HasEmployees = new(
        "Position.HasEmployees",
        "Cannot delete position that has assigned employees");
}
