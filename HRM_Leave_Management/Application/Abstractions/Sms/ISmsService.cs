using Domain.Abstractions;

namespace Application.Abstractions.Sms;

public interface ISmsService
{
    Task<Result> SentMessageAutoGenCodeAsync(string phoneNumber);
    Task<Result> CheckCodeAsync(string phoneNumber, string code);
    Task<Result> SendMessageCode(string code, string phoneNumber);
    Task<TResponse> SendRequestAsync<TResponse>(HttpRequestMessage requestMessage);
}