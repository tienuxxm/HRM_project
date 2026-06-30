using System.Net;
using System.Text;
using Application.Abstractions.VnPay;
using Application.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Infrastructure.VnPay;

public class VnPayService : IVnPayService
{
    public const string VERSION = "2.1.0";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly SortedList<string, string> _requestData = new(new VnPayCompare());
    private readonly SortedList<string, string> _responseData = new(new VnPayCompare());
    private readonly IOptions<VnPayOptions> _vnPayOptions;

    public VnPayService(IOptions<VnPayOptions> vnPayOptions, IHttpContextAccessor httpContextAccessor)
    {
        _vnPayOptions = vnPayOptions;
        _httpContextAccessor = httpContextAccessor;
        AddRequestData("vnp_Version", vnPayOptions.Value.Version);
        AddRequestData("vnp_Command", "pay");
        AddRequestData("vnp_TmnCode", vnPayOptions.Value.TmnCode);
        AddRequestData("vnp_CurrCode", "VND");
        AddRequestData("vnp_Locale", "vn");
        AddRequestData("vnp_OrderType", "180000"); //default value: other
        AddRequestData("vnp_ReturnUrl", vnPayOptions.Value.ReturnUrl);
    }

    public void AddRequestData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value)) _requestData.Add(key, value);
    }

    public void AddResponseData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value)) _responseData.Add(key, value);
    }

    public string GetResponseData(string key)
    {
        string retValue;
        if (_responseData.TryGetValue(key, out retValue))
            return retValue;
        return string.Empty;
    }

    public string CreateRequestUrl()
    {
        var data = new StringBuilder();
        var baseUrl = _vnPayOptions.Value.Url;
        var hashSecret = _vnPayOptions.Value.SecureHash;
        foreach (var kv in _requestData)
            if (!string.IsNullOrEmpty(kv.Value))
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");

        var queryString = data.ToString();

        baseUrl += "?" + queryString;
        var signData = queryString;
        if (signData.Length > 0) signData = signData.Remove(data.Length - 1, 1);

        var vnpSecureHash = Extention.HmacSHA512(hashSecret, signData);
        baseUrl += "vnp_SecureHash=" + vnpSecureHash;

        return baseUrl;
    }

    public bool ValidateSignature(string inputHash)
    {
        var rspRaw = GetResponseData();
        var myChecksum = Extention.HmacSHA512(_vnPayOptions.Value.SecureHash, rspRaw);
        return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
    }


    private string GetResponseData()
    {
        var data = new StringBuilder();
        if (_responseData.ContainsKey("vnp_SecureHashType")) _responseData.Remove("vnp_SecureHashType");

        if (_responseData.ContainsKey("vnp_SecureHash")) _responseData.Remove("vnp_SecureHash");

        foreach (var kv in _responseData)
            if (!string.IsNullOrEmpty(kv.Value))
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");

        //remove last '&'
        if (data.Length > 0) data.Remove(data.Length - 1, 1);

        return data.ToString();
    }
}