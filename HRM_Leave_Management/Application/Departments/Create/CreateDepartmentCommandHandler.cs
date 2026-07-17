using Application.Abstractions.Messaging;
using Application.Response;
using Domain.Abstractions;
using Domain.Departments;

namespace Application.Departments.Create;

internal sealed class CreateDepartmentCommandHandler : ICommandHandler<CreateDepartmentCommand, BooleanResponse>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDepartmentCommandHandler(IDepartmentRepository departmentRepository, IUnitOfWork unitOfWork)
    {
        _departmentRepository = departmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BooleanResponse>> Handle(CreateDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var isDepartmentExisted = await _departmentRepository.IsExistedAsync(x => x.Code.ToUpper() == normalizedCode);
        if (isDepartmentExisted)
            return Result.Failure<BooleanResponse>(DepartmentErrors.DepartmentExisted);

        DepartmentId? parentId = request.ParentDepartmentId.HasValue
            ? new DepartmentId(request.ParentDepartmentId.Value)
            : null;

        var department = Department.Create(
            request.Name,
            normalizedCode,
            request.Description,
            parentId);

        _departmentRepository.Add(department);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new BooleanResponse
            { Result = true, Message = $"Department: {request.Name} has been created" });
    }
}
