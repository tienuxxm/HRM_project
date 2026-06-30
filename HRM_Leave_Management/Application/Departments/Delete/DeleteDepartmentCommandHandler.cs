using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Abstractions;
using Domain.Departments;

namespace Application.Departments.Delete;

internal sealed class DeleteDepartmentCommandHandler : ICommandHandler<DeleteDepartmentCommand, BooleanResponse>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDepartmentCommandHandler(IDepartmentRepository departmentRepository, IUnitOfWork unitOfWork)
    {
        _departmentRepository = departmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BooleanResponse>> Handle(DeleteDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var department = await _departmentRepository.GetByIdAsync(new DepartmentId(request.Id));
        if (department is null)
            return Result.Failure<BooleanResponse>(DepartmentErrors.NotFound);

        // Check if department has children
        var hasChildren = await _departmentRepository.IsExistedAsync(
            x => x.ParentDepartmentId == department.Id);
        if (hasChildren)
            return Result.Failure<BooleanResponse>(DepartmentErrors.HasChildren);

        _departmentRepository.Remove(department);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new BooleanResponse
            { Result = true, Message = $"{request.Id} DELETED" });
    }
}
