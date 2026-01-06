using System;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Models;

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