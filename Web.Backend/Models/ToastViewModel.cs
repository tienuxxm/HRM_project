namespace Web.Backend.Models;

public enum ToastType
{
    Success,
    Error
}

public class ToastViewModelRequest
{
    public int ToastType { get; set; }
    public string Message { get; set; }
}

public class ToastViewModel
{
    public ToastType Type { get; set; }
    public string Message { get; set; }
}