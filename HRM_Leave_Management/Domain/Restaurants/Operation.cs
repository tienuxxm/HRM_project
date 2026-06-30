using System.ComponentModel;

namespace Domain.Restaurants;

public enum Operation
{
    [Description("Create")] Create = 0,
    [Description("Pending")] Pending = 1,
    [Description("Active")] Active = 2,
    [Description("InActive")] InActive = 3,
    [Description("Delete")] Deleted = 4,
}