using Application.Abstractions.Sms;
using Domain.Abstractions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Infrastructure.SmsServices;

public class SmsService : ISmsService
{
    private readonly string _apiUrl;
    private readonly IOptions<ESmsCheckCodeOptions> _eSmsCheckCodeV4Options;
    private readonly IOptions<ESmsGenCodeV4Options> _eSmsGenCodeV4Options;
    private readonly IOptions<ESmsOptions> _eSmsOptions;
    private readonly HttpClient _httpClient;

    public SmsService(HttpClient httpClient, IOptions<ESmsOptions> eSmsOptions,
        IOptions<ESmsGenCodeV4Options> eSmsGenCodeV4Options, IOptions<ESmsCheckCodeOptions> eSmsCheckCodeV4Options)
    {
        _httpClient = httpClient;
        _eSmsOptions = eSmsOptions;
        _eSmsGenCodeV4Options = eSmsGenCodeV4Options;
        _eSmsCheckCodeV4Options = eSmsCheckCodeV4Options;
        _apiUrl = _eSmsOptions.Value.ApiUrl;
    }

    public async Task<Result> SentMessageAutoGenCodeAsync(string phoneNumber)
    {
        var requestObj = (ESmsGenCodeV4Options)_eSmsGenCodeV4Options.Value.Clone();
        var requestMessage = new Dictionary<string, string>
        {
            { "Phone", phoneNumber },
            { "ApiKey", _eSmsOptions.Value.ApiKey },
            { "SecretKey", _eSmsOptions.Value.SecretKey },
            { "BrandName", _eSmsOptions.Value.Brandname },
            { "Type", requestObj.Type },
            { "NumCharOfCode", requestObj.NumCharOfCode },
            { "TimeAlive", requestObj.TimeAlive },
            { "message", requestObj.message }
        };
        var endPoint = requestObj.EndPoint;
        var request = BuildRequest(_apiUrl, requestMessage, endPoint, HttpMethod.Get);
        var response = await SendRequestAsync<ESmsReponse>(request);
        return response.CodeResult != "100" ? Result.Failure(ESmsErrors.SendCodeFail) : Result.Success();
    }

    public async Task<Result> CheckCodeAsync(string phoneNumber, string code)
    {
        var requestObj = _eSmsCheckCodeV4Options.Value;
        requestObj.Code = code;
        requestObj.PhoneNumber = phoneNumber;
        var endPoint = requestObj.EndPoint;
        var requestMessage = new Dictionary<string, string>
        {
            { "Phone", phoneNumber },
            { "ApiKey", _eSmsOptions.Value.ApiKey },
            { "SecretKey", _eSmsOptions.Value.SecretKey },
            { "Code", code }
        };


        var request = BuildRequest(_apiUrl, requestMessage, endPoint, HttpMethod.Get);
        var response = await SendRequestAsync<ESmsReponse>(request);
        return response.CodeResult != "100" ? Result.Failure(ESmsErrors.SendCodeFail) : Result.Success();
    }

    public async Task<Result> SendMessageCode(string code, string phone)
    {
        var content = _eSmsGenCodeV4Options.Value.message.Replace("{P,10}", code);
        var requestMessage = new Dictionary<string, string>
        {
            { "Phone", phone },
            { "Content", content },
            { "ApiKey", _eSmsOptions.Value.ApiKey },
            { "SecretKey", _eSmsOptions.Value.SecretKey },
            { "Brandname", _eSmsOptions.Value.Brandname },
            { "SmsType", "2" }
        };
        var request = BuildRequest(_apiUrl, requestMessage, "SendMultipleMessage_V4_get", HttpMethod.Get);
        var response = await SendRequestAsync<ESmsReponse>(request);
        return response.CodeResult != "100" ? Result.Failure(ESmsErrors.SendCodeFail) : Result.Success();
    }

    public async Task<TResponse> SendRequestAsync<TResponse>(HttpRequestMessage requestMessage)
    {
        var response = await _httpClient.SendAsync(requestMessage);
        var stringResult = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new ApplicationException("Request Send Fail" + stringResult);
        var result = JsonConvert.DeserializeObject<TResponse>(stringResult);
        return result;
    }

    private HttpRequestMessage BuildRequest(string baseUrl, IDictionary<string, string> requestMessage,
        string apiMethodEndpoint,
        HttpMethod method)
    {
        try
        {
            var url = baseUrl + apiMethodEndpoint;
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(url);
            request.Method = method;

            /*var requestDictionary = requestMessage.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => !string.IsNullOrEmpty((string)x.GetValue(requestMessage)))
                .ToDictionary(prop => prop.Name, prop => (string)prop.GetValue(requestMessage, null)!);*/
            var x = QueryHelpers.AddQueryString(url, requestMessage!);
            request.RequestUri = new Uri(x);

            return request;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}