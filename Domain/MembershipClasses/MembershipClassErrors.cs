using Domain.Abstractions;

namespace Domain.MembershipClasses;

public class MembershipClassErrors
{
    public static Error CreateFail => new("MembershipClass.Create.Fail", "Fail to create MembershipClass");

    public static Error NotFound => new("MembershipClass.NotFound",
        "The membership class with the specified identifier was not found");

    public static Error DeleteFail => new("MembershipClass.DeleteFail",
        "The membership class with the specified identifier is failed to delete");

    public static Error UpdateFail => new("MembershipClass.UpdateFail",
        "The membership class with the specified identifier is failed to update");

    public static Error Existed =>
        new("MembershipClass.Duplicated", "The membership class has been existed in the system");

    public static Error LevelExisted =>
        new("MembershipClass.LevelUnique", "The membership class level must be unique");
}