using Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IDbContext
{
    DbContext GetDbContext();
}