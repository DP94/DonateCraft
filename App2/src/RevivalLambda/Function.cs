using System.Text.Json.Serialization;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using System.Text;
using System.Text.Json;
using Common.Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RevivalLambda;

public class Function
{
    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public Function()
    {
    }


    public async Task HandleRequest(SQSEvent evnt, ILambdaContext context)
    {
        foreach (var record in evnt.Records)
        {
            var json = record.Body;
            var message = JsonSerializer.Deserialize<RevivalMessage>(json);
            Console.WriteLine(message.DonationId);
        }
    }
}