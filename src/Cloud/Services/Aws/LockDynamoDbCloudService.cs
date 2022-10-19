using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Cloud.Util;
using Common.Models;
using Common.Util;

namespace Cloud.Services.Aws;

public class LockDynamoDbCloudService : ILockCloudService
{

    private IAmazonDynamoDB _amazonDynamoDb;

    public LockDynamoDbCloudService(IAmazonDynamoDB amazonDynamoDb)
    {
        this._amazonDynamoDb = amazonDynamoDb;
    }

    public async Task<Lock> Create(Lock newLock)
    {
        await this._amazonDynamoDb.PutItemAsync(new PutItemRequest
        {
            TableName = DynamoDbConstants.LockTableName,
            Item = DynamoDbUtility.GetAttributesFromLock(newLock)
        });
        return newLock;
    }

    public async Task<List<Lock>> GetLocks()
    {
        var result = await this._amazonDynamoDb.ScanAsync(new ScanRequest(DynamoDbConstants.LockTableName));
        return result.Items.Select(DynamoDbUtility.GetLockFromAttributes).ToList();
    }

    public async Task<Lock> GetLock(string id)
    {
        var response = await this._amazonDynamoDb.GetItemAsync(new GetItemRequest
        {
            TableName = DynamoDbConstants.LockTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {
                    DynamoDbConstants.LockIdColName, new AttributeValue(id)
                }
            }
        });
        if (response.Item == null || response.Item.Count == 0)
        {
            throw new ResourceNotFoundException($"Lock {id} not found");
        }

        return DynamoDbUtility.GetLockFromAttributes(response.Item);
    }

    public async Task DeleteLock(string id)
    {
        await this._amazonDynamoDb.DeleteItemAsync(new DeleteItemRequest
        {
            TableName = DynamoDbConstants.LockTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {
                    DynamoDbConstants.LockIdColName, new AttributeValue(id)
                }
            }
        });
    }
}