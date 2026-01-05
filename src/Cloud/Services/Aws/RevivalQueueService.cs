using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Common.Models;
using Microsoft.Extensions.Options;

namespace Cloud.Services.Aws;

public class RevivalQueueService : IRevivalQueueService
{
    private IAmazonSQS _sqsClient;
    private readonly DonateCraftOptions _options;

    public RevivalQueueService(IAmazonSQS sqsClient, IOptions<DonateCraftOptions> options)
    {
        this._sqsClient = sqsClient;
        this._options = options.Value;
    }


    public async Task Enqueue(RevivalMessage message)
    {
        var json = JsonSerializer.Serialize(message);
        var request = new SendMessageRequest
        {
            QueueUrl = this._options.RevivalQueueUrl,
            MessageBody = json
        };
        await this._sqsClient.SendMessageAsync(request);
    }
}