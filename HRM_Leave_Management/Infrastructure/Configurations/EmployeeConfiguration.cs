using Domain.Departments;
using Domain.Employees;
using Domain.Positions;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employee");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => new EmployeeId(value));

        builder.Property(e => e.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.EmployeeCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(e => e.EmployeeCode).IsUnique();

        // Nullable FK — PositionId
        builder.Property(e => e.PositionId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                value => value.HasValue ? new PositionId(value.Value) : null);

        builder.HasOne(e => e.Position)
            .WithMany()
            .HasForeignKey(e => e.PositionId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("fk_employee_positions_position_temp_id1");

        // Nullable FK — DepartmentId
        builder.Property(e => e.DepartmentId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                value => value.HasValue ? new DepartmentId(value.Value) : null);

        builder.HasOne(e => e.Department)
            .WithMany()
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("fk_employee_department_department_temp_id1");

        // Nullable FK — UserId
        builder.Property(e => e.UserId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                value => value.HasValue ? new UserId(value.Value) : null);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.UserId)
            .IsUnique()
            .HasFilter("user_id IS NOT NULL")
            .HasDatabaseName("IX_employee_user_id");

        // Nullable FK — ManagerId (self-ref)
        builder.Property(e => e.ManagerId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                value => value.HasValue ? new EmployeeId(value.Value) : null);

        builder.HasOne(e => e.Manager)
            .WithMany()
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_employee_employee_manager_temp_id2");
    }
}
