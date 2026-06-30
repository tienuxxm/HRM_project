using System.Text.Json;
using Application.Abstractions.FirebaseMessaging;
using Application.Extensions;
using Domain.Abstractions;
using Domain.Members;
using Domain.SystemConfigurations;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;

namespace Infrastructure.Firebase;

public class FirebaseMessaging : IFirebaseMessaging
{
    private readonly IOptions<FirebaseOptions> _firebaseOptions;
    private readonly ISystemConfigurationRepository _systemConfigurationRepository;

    public FirebaseMessaging(IOptions<FirebaseOptions> firebaseOptions,
        ISystemConfigurationRepository systemConfigurationRepository)
    {
        _firebaseOptions = firebaseOptions;
        _systemConfigurationRepository = systemConfigurationRepository;

        InitFirebase();
    }

    private void InitFirebase()
    {
        try
        {
            if (FirebaseApp.DefaultInstance is not null && FirebaseApp.DefaultInstance.Name == "[DEFAULT]") return;
            var config = _systemConfigurationRepository.GetConfigByName(new ConfigName("Firebase")).Result;
            if (config is null)
                return;
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromJson(config.ConfigJsonValue.Value),
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Result> SendNotification(string deviceToken, string message)
    {
        try
        {
            var notificationMessage = new Message()
            {
                Token = deviceToken,
                Notification = new Notification()
                {
                    Body = message,
                }
            };
            var response =
                await FirebaseAdmin.Messaging.FirebaseMessaging.DefaultInstance.SendAsync(notificationMessage);
            return string.IsNullOrEmpty(response) ? Result.Failure(Error.None) : Result.Success(response);
        }
        catch (Exception e)
        {
            return Result.Failure(Error.None);
        }
    }

    public async Task<Result> SendMultipleNotification(List<FirebaseMessageRequest> messageRequests)
    {
        try
        {
            var listMessage = messageRequests.Select(request => new Message()
            {
                Token = request.DeviceToken,
                Notification = new Notification()
                {
                    Body = request.Message,
                }
            });
            var response =
                await FirebaseAdmin.Messaging.FirebaseMessaging.DefaultInstance.SendAllAsync(listMessage);
            return Result.Success(response);
        }
        catch (Exception e)
        {
            return Result.Failure(Error.None);
        }
    }
}