using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Cloud.Util;
using Common.Models;
using Common.Util;

namespace Cloud.Services;

public class DeathDynamoDbStorageService : IDeathCloudService
{
    private readonly IAmazonDynamoDB _dynamoDb;
    
    public DeathDynamoDbStorageService(IAmazonDynamoDB dynamoDb)
    {
        this._dynamoDb = dynamoDb;
    }

    public async Task<List<Death>> GetDeaths()
    {
        var result = await this._dynamoDb.ScanAsync(new ScanRequest(DynamoDbConstants.DeathTableName));
        return result.Items.Select(DynamoDbUtility.GetDeathFromAttributes).ToList();
    }

    public async Task<Death?> GetDeathById(string id)
    {
        var response = await this._dynamoDb.GetItemAsync(new GetItemRequest
        {
            TableName = DynamoDbConstants.DeathTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {
                    DynamoDbConstants.DeathIdColName, new AttributeValue(id)
                }
            }
        });
        if (response.Item == null || response.Item.Count == 0)
        {
            throw new ResourceNotFoundException($"Death {id} not found");
        }

        return DynamoDbUtility.GetDeathFromAttributes(response.Item);
    }

    public async Task<Death> CreateDeath(Death death)
    {
        await this._dynamoDb.PutItemAsync(new PutItemRequest
        {
            TableName = DynamoDbConstants.DeathTableName,
            Item = DynamoDbUtility.GetAttributesFromDeath(death)
        });
        return death;
    }

    public async Task DeleteDeath(string id)
    {
        await this._dynamoDb.DeleteItemAsync(new DeleteItemRequest
        {
            TableName = DynamoDbConstants.DeathTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {
                    DynamoDbConstants.DeathIdColName, new AttributeValue(id)
                }
            }
        });
    }

    public async Task<Death> UpdateDeath(Death death)
    {
        await this._dynamoDb.PutItemAsync(new PutItemRequest
        {
            TableName = DynamoDbConstants.DeathTableName,
            Item = DynamoDbUtility.GetAttributesFromDeath(death)
        });
        return death;
    }
}