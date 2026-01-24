using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using System.Text.Json;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Cloud.Services;
using Cloud.Services.Aws;
using Common.Models;
using Common.Util;
using Core.Services.Charity;
using Core.Services.Donation;
using Core.Services.Lock;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RevivalLambda.Services;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RevivalLambda;

public class Function
{

    private IRevivalService _revivalService;
    private ILogger<Function> _logger;
    
    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public Function()
    {
        var provider = ConfigureServices();
        this._revivalService =  provider.GetService<IRevivalService>();
        this._logger = provider.GetService<ILogger<Function>>();
    }
    
    public async Task HandleRequest(SQSEvent evnt)
    {
        foreach (var record in evnt.Records)
        {
            try
            {
                var json = record.Body;
                var message = JsonSerializer.Deserialize<RevivalMessage>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                await this._revivalService.ProcessRevival(message);
            }
            catch (Exception e)
            {
                this._logger.LogWarning("Unable to process revival {Message}", e.Message);
            }

        }
    }


    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILockService, LockService>();
        services.AddSingleton<ICharityService, CharityService>();
        services.AddSingleton<IDonationService, DonationService>();
        services.AddSingleton<ILockCloudService, LockDynamoDbCloudService>();
        services.AddSingleton<ICharityCloudService, CharityDynamoDbCloudService>();
        services.AddSingleton<IDonationCloudService, DonationDynamoDbCloudService>();
        services.AddSingleton<IPlayerCloudService, PlayerDynamoDbCloudService>();
        services.AddSingleton<IRevivalService, RevivalService>();
        services.AddAWSService<IAmazonDynamoDB>(new AWSOptions
        {
            Region = RegionEndpoint.EUWest2
        });
        services.AddMemoryCache();
        services.AddLogging();
        var client = new HttpClient { BaseAddress = new Uri(Environment.GetEnvironmentVariable(Constants.JG_API_URL) ??
                                                            "https://api.staging.justgiving.com/") };
        services.AddSingleton(client);

        var options = new DonateCraftOptions();
        var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build().GetSection("DonateCraft");
        configuration.Bind(options);
        var wrapper = Options.Create(options);
        services.AddSingleton(wrapper);
        return services.BuildServiceProvider();
    }
}