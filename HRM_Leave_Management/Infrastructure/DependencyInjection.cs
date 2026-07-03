using Amazon.Runtime;
using Amazon.S3;
using Application.Abstractions.Authentication;
using Application.Abstractions.AWS;
using Application.Abstractions.Clock;
using Application.Abstractions.Data;
using Application.Abstractions.Email;
using Application.Abstractions.FirebaseMessaging;
using Application.Abstractions.GoogleAuth2;
using Application.Abstractions.Role;
using Application.Abstractions.Sms;
using Application.Abstractions.VnPay;
using Dapper;
using Domain.Abstractions;
using Domain.Bookings;
using Domain.Categories;
using Domain.Departments;
using Domain.Deliveries;
using Domain.Employees;
using Domain.Positions;
using Domain.Districts;
using Domain.FreeServices;
using Domain.Images;
using Domain.Invoices;
using Domain.MemberActivities;
using Domain.MemberDeviceTokens;
using Domain.MemberNotifications;
using Domain.MemberPointHistories;
using Domain.MemberPointRules;
using Domain.Members;
using Domain.MembershipBenefits;
using Domain.MembershipClasses;
using Domain.MemberVouchers;
using Domain.News;
using Domain.Notifications;
using Domain.OrderFees;
using Domain.Orders;
using Domain.Partners;
using Domain.Permissions;
using Domain.PhoneValidationCheck;
using Domain.ProductOfRestaurants;
using Domain.Products;
using Domain.Promotions;
using Domain.PromotionToRestaurants;
using Domain.Provinces;
using Domain.QrCode;
using Domain.RestaurantAreas;
using Domain.Restaurants;
using Domain.Reviews;
using Domain.Roles;
using Domain.RoleToPermissions;
using Domain.SystemConfigurations;
using Domain.SystemLog;
using Domain.Users;
using Domain.LeaveTypes;
using Domain.LeaveBalances;
using Domain.LeaveRequests;
using Domain.LeaveApproverAssignments;
using Domain.UserToRoles;
using Domain.Vouchers;
using Infrastructure.Authentication;
using Infrastructure.Clock;
using Infrastructure.Data;
using Infrastructure.Email;
using Infrastructure.Firebase;
using Infrastructure.Jobs;
using Infrastructure.Outbox;
using Infrastructure.Repositories;
using Infrastructure.RoleServices;
using Infrastructure.SmsServices;
using Infrastructure.VnPay;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Quartz;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<IDateTimeProvider, DateTimeProvider>();

        services.AddTransient<IEmailService, EmailService>();

        AddDataProtection(services);

        ConfigureAws(services, configuration);

        ConfigureVnPay(services, configuration);

        ConfigureRoleService(services, configuration);

        AddPersistence(services, configuration);

        AddConfiguration(services, configuration);

        AddAuthentication(services, configuration);

        AddBackgroundJobs(services, configuration);

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("Database") ??
            throw new ArgumentNullException(nameof(configuration));

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IMemberRepository, MemberRepository>();

        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddScoped<IRestaurantRepository, RestaurantsRepository>();

        services.AddScoped<IBookingRepository, BookingRepository>();

        services.AddScoped<IReviewRepository, ReviewRepository>();

        services.AddScoped<ICategoryRepository, CategoryRepository>();

        services.AddScoped<IDepartmentRepository, DepartmentRepository>();

        services.AddScoped<IPositionRepository, PositionRepository>();

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();

        services.AddScoped<ILeaveTypeRepository, LeaveTypeRepository>();

        services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>();

        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();

        services.AddScoped<ILeaveApproverAssignmentRepository, LeaveApproverAssignmentRepository>();

        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddScoped<INewsRepository, NewsRepository>();

        services.AddScoped<IQrCodeRepository, QrCodeRepository>();

        services.AddScoped<IMemberPointHistoryRepository, MemberPointHistoryRepository>();


        services.AddScoped<IDeliveryRepository, DeliveryRepository>();

        services.AddScoped<IImageRepository, ImageRepository>();

        services.AddScoped<IPartnerRepository, PartnerRepository>();

        services.AddScoped<IVoucherRepository, VoucherRepository>();

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IMembershipBenefitRepository, MembershipBenefitRepository>();

        services.AddScoped<IRoleRepository, RoleRepository>();

        services.AddScoped<ISystemConfigurationRepository, SystemConfigurationRepositoryRepository>();

        services.AddScoped<IMembershipClassRepository, MembershipClassRepository>();

        services.AddScoped<IUserToRoleRepository, UserToRoleRepository>();

        services.AddScoped<IMemberPointRuleRepository, MemberPointRuleRepository>();

        services.AddScoped<IPhoneValidationCheckRepository, PhoneValidationCheckRepository>();

        services.AddScoped<IInvoiceRepository, InvoiceRepository>();

        services.AddScoped<IMemberVoucherRepository, MemberVoucherRepository>();

        services.AddScoped<IPromotionRepository, PromotionRepository>();

        services.AddScoped<IFeeServiceRepository, FeeServiceRepository>();

        services.AddScoped<INotificationRepository, NotificationRepository>();

        services.AddScoped<IPromotionToRestaurantRepository, PromotionToRestaurantRepository>();

        services.AddScoped<IFirebaseMessaging, FirebaseMessaging>();

        services.AddScoped<IMemberDeviceTokenRepository, MemberDeviceTokenRepository>();

        services.AddScoped<IMemberNotificationRepository, MemberNotificationRepository>();

        services.AddScoped<IRestaurantAreaRepository, RestaurantAreaRepository>();

        services.AddScoped<IPermissionRepository, PermissionRepository>();

        services.AddScoped<ILineItemRepository, LineItemRepository>();

        services.AddScoped<IMemberActivityRepository, MemberActivityRepository>();

        services.AddScoped<IOrderFeeRespository, OrderFeeRepository>();

        services.AddScoped<IProductOfRestaurantRepository, ProductOfRestaurantRepository>();

        services.AddScoped<IProvinceRepository, ProvinceRepository>();

        services.AddScoped<IDistrictRepository, DistrictRepository>();

        services.AddScoped<ISystemLogRepository, SystemLogRepository>();

        services.AddScoped<IGoogleAuth2, GoogleAuth2>();

        services.AddScoped<IRoleToPermissionRepository, RoleToPermissionRepository>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddSingleton<ISqlConnectionFactory>(_ =>
            new SqlConnectionFactory(connectionString));

        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
    }

    private static void ConfigureAws(IServiceCollection services, IConfiguration configuration)
    {
        var option = configuration.GetAWSOptions();
        var accessKey = configuration.GetSection("AWS").GetSection("AccessKey").Value;
        var secretKey = configuration.GetSection("AWS").GetSection("SecretKey").Value;
        option.Credentials = new BasicAWSCredentials(accessKey, secretKey);
        services.AddDefaultAWSOptions(option);
        services.AddAWSService<IAmazonS3>(option);
        services.AddScoped<IAwsS3Service, AwsS3Service>();
    }

    private static void ConfigureVnPay(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<VnPayOptions>(configuration.GetSection("VnPay"));
        services.AddScoped<IVnPayService, VnPayService>();
    }

    private static void ConfigureRoleService(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<VnPayOptions>(configuration.GetSection("Role"));
        services.AddScoped<IRoleService, RoleService>();
    }

    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        IdentityModelEventSource.ShowPII = true; //Add this line

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();
        services.Configure<AuthenticationOptions>(configuration.GetSection("Authentication"));

        services.ConfigureOptions<JwtBearerOptionsSetup>();

        services.Configure<KeycloakOptions>(configuration.GetSection("Keycloak"));


        services.AddTransient<AdminAuthorizationDelegatingHandler>();

        services.AddHttpClient<IAuthenticationService, AuthenticationService>((serviceProvider, httpClient) =>
            {
                var keycloakOptions = serviceProvider.GetRequiredService<IOptions<KeycloakOptions>>().Value;

                httpClient.BaseAddress = new Uri(keycloakOptions.AdminUrl);
            })
            .AddHttpMessageHandler<AdminAuthorizationDelegatingHandler>();

        services.AddHttpClient<ISmsService, SmsService>((serviceProvider, httpClient) =>
        {
            var eSmsOption = serviceProvider.GetRequiredService<IOptions<ESmsOptions>>().Value;
            httpClient.BaseAddress = new Uri(eSmsOption.ApiUrl);
        });

        services.AddHttpClient<IJwtService, JwtService>((serviceProvider, httpClient) =>
        {
            var keycloakOptions = serviceProvider.GetRequiredService<IOptions<KeycloakOptions>>().Value;

            httpClient.BaseAddress = new Uri(keycloakOptions.TokenUrl);
        });

        services.AddHttpContextAccessor();

        services.AddScoped<IMemberContext, MemberContext>();
        services.AddScoped<IUserContext, UserContext>();
    }

    private static void AddConfiguration(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ESmsOptions>(configuration.GetSection("ESms"));
        services.Configure<EmailOptions>(configuration.GetSection("SendGrid"));
        services.Configure<GoogleAuth2Options>(configuration.GetSection("GoogleAuth2Option"));
        services.Configure<ESmsGenCodeV4Options>(configuration.GetSection("ESms").GetSection("Method")
            .GetSection("GenCodeV4"));
        services.Configure<ESmsCheckCodeOptions>(configuration.GetSection("ESms").GetSection("Method")
            .GetSection("CheckCodeV4"));

        services.Configure<FirebaseOptions>(configuration.GetSection("Firebase"));
    }

    private static void AddDataProtection(IServiceCollection services)
    {
        services.AddDataProtection()
            .SetDefaultKeyLifetime(TimeSpan.FromDays(14));
    }

    private static void AddBackgroundJobs(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OutboxOptions>(configuration.GetSection("Outbox"));

        services.AddQuartz(options =>
        {
            options.UseMicrosoftDependencyInjectionJobFactory();

            options.AddJob<DailyJob>(opts => opts.WithIdentity("DailyJob"));
            options.AddJob<YearEndJob>(opts => opts.WithIdentity("YearEndJob"));

            // options.AddTrigger(opts => opts
            //     .ForJob("DailyJob")
            //     .WithIdentity("DailyJobTrigger")
            //     .WithSimpleSchedule(x => x.WithIntervalInHours(2).RepeatForever()));
            options.AddTrigger(opts => opts
                .ForJob("DailyJob")
                .WithIdentity("DailyJobTrigger")
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInHours(1).RepeatForever()));
            options.AddTrigger(opts => opts
                .ForJob("YearEndJob")
                .WithIdentity("YearEndJobTrigger")
                .StartNow()
                .WithCronSchedule("0 59 23 31 12 ? *"));
            // .WithCronSchedule("0 0 4 * * ?"));
            // .WithCronSchedule("0 0 4 * * ?"));
        });

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        services.ConfigureOptions<ProcessOutboxMessagesJobSetup>();
    }
}