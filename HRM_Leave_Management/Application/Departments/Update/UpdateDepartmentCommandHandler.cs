using Application.Abstractions.Messaging;
using Domain.Abstractions;
using Domain.Departments;

namespace Application.Departments.Update;

internal sealed class UpdateDepartmentCommandHandler : ICommandHandler<UpdateDepartmentCommand, Department>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDepartmentCommandHandler(IDepartmentRepository departmentRepository, IUnitOfWork unitOfWork)
    {
        _departmentRepository = departmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Department>> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _departmentRepository.GetByIdAsync(new DepartmentId(request.Id));
        if (department is null)
        {
            return Result.Failure<Department>(DepartmentErrors.NotFound);
        }

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        // Check duplicate code (exclude self)
        var isDuplicate = await _departmentRepository.IsExistedAsync(
            x => x.Code.ToUpper() == normalizedCode && x.Id != new DepartmentId(request.Id));
        if (isDuplicate)
            return Result.Failure<Department>(DepartmentErrors.DepartmentExisted);

        DepartmentId? parentId = request.ParentDepartmentId.HasValue
            ? new DepartmentId(request.ParentDepartmentId.Value)
            : null;

        department.Update(request.Name, normalizedCode, request.Description, parentId);

        _departmentRepository.Update(department);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return department;
    }
}
