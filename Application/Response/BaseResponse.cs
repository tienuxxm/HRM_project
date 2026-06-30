namespace Application.Response;

public abstract class BaseResponse<T>
{
    public T Result { get; set; }
    public bool HasError { get; set; }
    public string Message { get; set; }
    public string Error { get; set; }
}