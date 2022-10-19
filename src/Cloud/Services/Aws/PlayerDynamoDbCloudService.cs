using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Cloud.Util;
using Common.Models;
using Common.Util;

namespace Cloud.Services.Aws;

public class PlayerDynamoDbCloudService : IPlayerCloudService
{
    
    private readonly IAmazonDynamoDB _dynamoDb;
    
    public PlayerDynamoDbCloudService(IAmazonDynamoDB dynamoDb)
    {
        this._dynamoDb = dynamoDb;
    }
    
    public async Task<List<Player>> GetPlayers()
    {
        var result = await this._dynamoDb.ScanAsync(new ScanRequest(DynamoDbConstants.PlayerTableName));
        return result.Items.Select(DynamoDbUtility.GetPlayerFromAttributes).ToList();
    }

    public async Task<Player?> GetPlayerById(string id)
    {
        var response = await this._dynamoDb.GetItemAsync(new GetItemRequest
        {
            TableName = DynamoDbConstants.PlayerTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {
                    DynamoDbConstants.PlayerIdColName, new AttributeValue(id)
                }
            }
        });
        if (response.Item == null || response.Item.Count == 0)
        {
            throw new ResourceNotFoundException($"Player {id} not found");
        }

        return DynamoDbUtility.GetPlayerFromAttributes(response.Item);
    }

    public async Task<Player> CreatePlayer(Player player)
    {
        await this._dynamoDb.PutItemAsync(new PutItemRequest
        {
            TableName = DynamoDbConstants.PlayerTableName,
            Item = DynamoDbUtility.GetAttributesFromPlayer(player)
        });
        return player;
    }

    public async Task DeletePlayer(string id)
    {
        await this._dynamoDb.DeleteItemAsync(new DeleteItemRequest
        {
            TableName = DynamoDbConstants.PlayerTableName,
            Key = new Dictionary<string, AttributeValue>
            {
                {
                    DynamoDbConstants.PlayerIdColName, new AttributeValue(id)
                }
            }
        });
    }

    public async Task<Player> UpdatePlayer(Player player)
    {
        await this._dynamoDb.PutItemAsync(new PutItemRequest
        {
            TableName = DynamoDbConstants.PlayerTableName,
            Item = DynamoDbUtility.GetAttributesFromPlayer(player)
        });
        return player;
    }
}