namespace Application.Abstractions.VnPay;

public interface IVnPayService
{
    public void AddRequestData(string key, string value);

    public void AddResponseData(string key, string value);

    public string GetResponseData(string key);

    public string CreateRequestUrl();

    public bool ValidateSignature(string inputHash);
}