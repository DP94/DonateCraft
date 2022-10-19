using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.Runtime;
using Cloud.Services;
using Cloud.Services.Aws;
using Common.Models;
using Common.Util;
using Core.Services;
using Microsoft.Extensions.DependencyInjection;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace DeathLambda;

public class DeathLambda
{
    private readonly ILockService _lockService;

    public DeathLambda()
    {
        var serviceCollection = ConfigureLambda();
        this._lockService = serviceCollection.GetService<ILockService>();
    }

    public DeathLambda(ILockService lockService)
    {
        this._lockService = lockService;
    }
    
    public async Task HandleRequest(DynamoDBEvent ddbEvent, ILambdaContext context)
    {
        foreach (var record in ddbEvent.Records)
        {
            if (record.EventName != OperationType.INSERT)
            {
                LambdaLogger.Log($"Event is not INSERT - skipping this record");
                continue;
            }
            
            if (record.Dynamodb.NewImage.TryGetValue(DynamoDbConstants.DeathPlayerIdColName, out var playerId))
            {
                LambdaLogger.Log($"Creating a new lock for player {playerId}!");
                await this._lockService.Create(new Lock
                {
                    Id = Guid.NewGuid().ToString(),
                    Unlocked = false,
                    Key = playerId.S
                });
                LambdaLogger.Log("Lock created");
            }
        }
    }

    private static IServiceProvider ConfigureLambda()
    {
        var serviceCollection = new ServiceCollection();
        
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var awsOptions = new AWSOptions();
        if (env == "LOCAL")
        {
            awsOptions.Credentials = new BasicAWSCredentials("x", "x");
            awsOptions.DefaultClientConfig.ServiceURL = "http://localhost:8000";    
        }
        serviceCollection.AddDefaultAWSOptions(awsOptions);
        serviceCollection.AddAWSService<IAmazonDynamoDB>(awsOptions);
        serviceCollection.AddSingleton<ILockService, LockService>();
        serviceCollection.AddSingleton<ILockCloudService, LockDynamoDbCloudService>();
        return serviceCollection.BuildServiceProvider();
    }
}
