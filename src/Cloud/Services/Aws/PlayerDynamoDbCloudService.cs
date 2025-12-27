using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Cloud.Util;
using Common.Exceptions;
using Common.Models;
using Common.Util;
using Microsoft.Extensions.Options;
using ResourceNotFoundException = Common.Exceptions.ResourceNotFoundException;

namespace Cloud.Services.Aws;

public class PlayerDynamoDbCloudService : IPlayerCloudService
{
    
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly DonateCraftOptions _options;
    
    public PlayerDynamoDbCloudService(IAmazonDynamoDB dynamoDb, IOptions<DonateCraftOptions> options)
    {
        this._dynamoDb = dynamoDb;
        this._options = options.Value;
    }

    public async Task<List<Player>> GetPlayers()
    {
        var result = await this._dynamoDb.ScanAsync(new ScanRequest(this._options.PlayerTableName));
        var criteria = new DynamoAttributeMappingCriteria(true, true);
        return result.Items.Select(item =>
            DynamoDbUtility.GetPlayerFromAttributes(item, criteria)).ToList();
    }

    public async Task<Player> GetPlayerById(string id)
    {
        return await this.GetPlayerById(id, new DynamoAttributeMappingCriteria());
    }
    
    public async Task<Player> GetPlayerById(string id, DynamoAttributeMappingCriteria criteria)
    {
        var response = await this._dynamoDb.GetItemAsync(new GetItemRequest
        {
            TableName = this._options.PlayerTableName,
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

        return DynamoDbUtility.GetPlayerFromAttributes(response.Item, criteria);
    }

    public async Task<Player> CreatePlayer(Player player)
    {
        try
        {
            await this._dynamoDb.PutItemAsync(new PutItemRequest
            {
                TableName = this._options.PlayerTableName,
                Item = DynamoDbUtility.GetAttributesFromPlayer(player),
                ConditionExpression = $"attribute_not_exists({DynamoDbConstants.PlayerIdColName})"
            });
        }
        catch (ConditionalCheckFailedException e)
        {
            throw new ResourceExistsException($"Player with id {player.Id} already exists!");
        }

        return player;
    }

    public async Task DeletePlayer(string id)
    {
        await this._dynamoDb.DeleteItemAsync(new DeleteItemRequest
        {
            TableName = this._options.PlayerTableName,
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
        await this.GetPlayerById(player.Id);
        await this._dynamoDb.PutItemAsync(new PutItemRequest
        {
            TableName = this._options.PlayerTableName,
            Item = DynamoDbUtility.GetAttributesFromPlayer(player)
        });
        return player;
    }
}