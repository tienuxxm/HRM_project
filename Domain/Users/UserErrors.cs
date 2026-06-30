using Domain.Abstractions;

namespace Domain.Users;

public class UserErrors
{
    public static Error NotFound = new(
        "User.Found",
        "The member with the specified identifier was not found");

    public static Error InvalidCredentials = new(
        "User.InvalidCredentials",
        "The provided credentials were invalid");

    public static Error DuplicateEmail = new(
        "User.DuplicateEmail",
        "The Email already exist");

    public static Error DuplicateUsername = new(
        "User.DuplicateEmail",
        "The Username already exist");
}